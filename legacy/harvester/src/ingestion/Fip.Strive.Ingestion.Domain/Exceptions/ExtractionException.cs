namespace Fip.Strive.Ingestion.Domain.Exceptions;

public class ExtractionException : Exception
{
    public ExtractionException() { }

    public ExtractionException(string filepath, string message)
        : base($"{filepath} | Extraction | {message}") { }

    public ExtractionException(string filepath, string message, Exception exception)
        : base(
            $"{filepath} | Extraction | {message} | {exception.GetType().Name} | {exception.Message}",
            exception
        ) { }
}
