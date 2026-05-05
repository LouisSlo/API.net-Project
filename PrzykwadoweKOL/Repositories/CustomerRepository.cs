using Microsoft.Data.SqlClient;
using PrzykwadoweKOL.DTOs;


namespace PrzykwadoweKOL.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly string _connectionString;

    public CustomerRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<CustomerRentalDTO> GetCustomerWithRentalsAsync(int customerId)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        // 1. ZAPYTANIE SQL - Zabezpieczone parametrem @Id przed włamaniami (SQL Injection)
        var sql = @"
            SELECT 
                c.first_name, 
                c.last_name, 
                r.rental_id, 
                r.rental_date, 
                r.return_date, 
                s.name AS status_name, 
                m.title, 
                ri.price_at_rental
            FROM Customer c
            JOIN Rental r ON c.customer_id = r.customer_id
            JOIN Status s ON r.status_id = s.status_id
            JOIN Rental_Item ri ON r.rental_id = ri.rental_id
            JOIN Movie m ON ri.movie_id = m.movie_id
            WHERE c.customer_id = @Id";

        // 2. PRZYGOTOWANIE KOMENDY
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", customerId);

        // 3. ODCZYT DANYCH Z BAZY (wiersz po wierszu)
        await using var reader = await command.ExecuteReaderAsync();

        // Zmienna na nasz główny wynik (na razie pusta)
        CustomerRentalDTO result = null;

        while (await reader.ReadAsync())
        {
            // Jeśli czytamy pierwszy wiersz, tworzymy główny obiekt klienta
            if (result == null)
            {
                result = new CustomerRentalDTO
                {
                    FirstName = reader["first_name"].ToString(),
                    LastName = reader["last_name"].ToString(),
                    Rentals = new List<RentalDTO>() // Inicjujemy pustą listę wypożyczeń
                };
            }

            // Pobieramy ID wypożyczenia z obecnego wiersza
            int rentalId = Convert.ToInt32(reader["rental_id"]);

            // Sprawdzamy, czy to wypożyczenie zostało już dodane do listy klienta
            var rental = result.Rentals.FirstOrDefault(r => r.Id == rentalId);

            // Jeśli nie, tworzymy nowe wypożyczenie
            if (rental == null)
            {
                rental = new RentalDTO
                {
                    Id = rentalId,
                    RentalDate = Convert.ToDateTime(reader["rental_date"]),
                    // return_date może być NULL w bazie danych, musimy to ostrożnie obsłużyć
                    ReturnDate = reader["return_date"] == DBNull.Value ? null : Convert.ToDateTime(reader["return_date"]),
                    Status = reader["status_name"].ToString(),
                    Movies = new List<MovieDTO>() // Inicjujemy pustą listę filmów dla tego wypożyczenia
                };
                result.Rentals.Add(rental);
            }

            // Na koniec, do obecnego wypożyczenia dodajemy film z tego wiersza
            rental.Movies.Add(new MovieDTO
            {
                Title = reader["title"].ToString(),
                PriceAtRental = Convert.ToDecimal(reader["price_at_rental"])
            });
        }

        // Zwracamy poskładany obiekt (lub null, jeśli klient o podanym ID nie istniał)
        return result;
    }

    public async Task<int> AddRentalAsync(int customerId, CreateRentalRequestDTO request)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        //Sprawdzanie czy klient istnieje 
        var checkCustomerSql = " SELECT Count(1) From Customer Where customer_id = @Id";
        await using var checkCmd = new SqlCommand(checkCustomerSql, connection);
        checkCmd.Parameters.AddWithValue("@Id", customerId);

        int customerExist = (int)await checkCmd.ExecuteScalarAsync();
        if (customerExist == 0)
        {
            throw new Exception("Customer not found");
        }

        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();

        try
        {
            var insertRentalSql = @"
Insert into Rental  (rental_date , customer_id, status_id)
OUTPUT Inserted.rental_id   
Values (@Date ,@CostId, 1)
";
            await using var insertRentalCmd = new SqlCommand(insertRentalSql, connection, transaction);
            insertRentalCmd.Parameters.AddWithValue("@Date", request.rentalDate);
            insertRentalCmd.Parameters.AddWithValue("@CostId", customerId);

            int newRentalId = (int)await insertRentalCmd.ExecuteScalarAsync();

            foreach (var movie in request.movies)
            {
                var findMovieSql = "SELECT movie_id FROM Movie WHERE title = @Title ";
                await using var findMovieCmd = new SqlCommand(findMovieSql, connection, transaction);
                findMovieCmd.Parameters.AddWithValue("@Title", movie.title);

                var movieIdResult = await findMovieCmd.ExecuteScalarAsync();

                if (movieIdResult == null)
                {
                    throw new Exception($"Movie {movie.title} not found");
                }

                int movieId = (int)movieIdResult;

                var insertItemSql = @"INSERT INTO Rental_Item (rental_id, movie_id, price_at_rental) 
                    VALUES (@RentalId, @MovieId, @Price)";
                await using var insertItemCmd = new SqlCommand(insertItemSql, connection, transaction);
                insertItemCmd.Parameters.AddWithValue("@RentalId", newRentalId);
                insertItemCmd.Parameters.AddWithValue("@MovieId", movieId);
                insertItemCmd.Parameters.AddWithValue("@Price", movie.rentalPrice);

                await insertItemCmd.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
            return newRentalId;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new Exception(ex.Message);
        }

    }
    
}