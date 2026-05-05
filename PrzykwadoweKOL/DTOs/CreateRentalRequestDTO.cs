namespace PrzykwadoweKOL.DTOs;

public class CreateRentalRequestDTO
{
    public DateTime rentalDate { get; set; }

    public List<CreateRentalMoviesDTO> movies { get; set; } = new List<CreateRentalMoviesDTO>();
}

public class CreateRentalMoviesDTO
{
    public string title  { get; set; } = string.Empty;
    public decimal rentalPrice { get; set; }
    
}