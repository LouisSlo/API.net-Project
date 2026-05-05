namespace PrzykwadoweKOL.Exceptions;

// Dziedziczymy po klasie Exception (tworzymy jej wyspecjalizowaną wersję)
public class NotFoundException : Exception
{
    // Przekazujemy wiadomość do bazowego Exception
    public NotFoundException(string message) : base(message)
    {
    }
}