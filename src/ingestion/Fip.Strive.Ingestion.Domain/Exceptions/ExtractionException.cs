namespace Fip.Strive.Ingestion.Domain.Exceptions;

public class ExtractionException : Exception
{
    public ExtractionException() { }

    public ExtractionException(string filepath, string message)
        : base($"{filepath} | Extraction | {message}") { }
}
