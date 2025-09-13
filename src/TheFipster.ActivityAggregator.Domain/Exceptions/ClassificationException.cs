namespace TheFipster.ActivityAggregator.Domain.Exceptions;

public class ClassificationException : Exception
{
    public ClassificationException() { }

    public ClassificationException(string filepath, string message)
        : base($"{filepath}: {message}") { }
}
