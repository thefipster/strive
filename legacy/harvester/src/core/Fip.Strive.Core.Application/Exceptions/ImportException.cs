namespace Fip.Strive.Core.Domain.Exceptions;

public class ImportException : Exception
{
    public ImportException(string filename, string message)
        : base($"{filename}: {message}") { }

    public ImportException(string filename, string message, Exception exception)
        : base(
            $"{filename}: {message} | {exception.GetType().Name}: {exception.Message}",
            exception
        ) { }
}
