namespace Fip.Strive.Core.Domain.Exceptions;

public class UploadException : Exception
{
    public UploadException(string filename, string message)
        : base($"{filename}: {message}") { }

    public UploadException(string filename, string message, Exception exception)
        : base(
            $"{filename}: {message} | {exception.GetType().Name}: {exception.Message}",
            exception
        ) { }
}
