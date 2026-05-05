namespace PrzykwadoweKOL.DTOs;

public class CustomerRentalDTO
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public List<RentalDTO> Rentals { get; set; } = new List<RentalDTO>();
}

public class RentalDTO
{
    public int Id { get; set; }
    public DateTime RentalDate { get; set; }
    
    // ZNAK ZAPYTANIA JEST TU KLUCZOWY! (Wyjaśnienie niżej)
    public DateTime? ReturnDate { get; set; } 
    
    public string Status { get; set; } = string.Empty;
    public List<MovieDTO> Movies { get; set; } = new List<MovieDTO>();
}

public class MovieDTO
{
    public string Title { get; set; } = string.Empty;
    public decimal PriceAtRental { get; set; }
}