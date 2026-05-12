namespace PrzykwadoweKOL.Exceptions;

// Nasza klasa musi dziedziczyć po głównej klasie Exception
public class DatabaseOperationException : Exception
{
    // Konstruktor przyjmujący tylko wiadomość
    public DatabaseOperationException(string message) : base(message)
    {
    }

    // Konstruktor przyjmujący wiadomość ORAZ oryginalny wyjątek (InnerException)
    // To jest kluczowe dla prowadzącego - żeby nie zgubić szczegółów błędu z bazy!
    public DatabaseOperationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}