namespace TheFipster.ActivityAggregator.Domain.Exceptions;

public class ClassificationException : Exception
{
    public ClassificationException() { }

    public ClassificationException(string filepath, string message)
        : base($"{filepath} | Classification | {message}") { }

    public ClassificationException(string filepath, DataSources classifier, string message)
        : base($"{filepath} | Classification | {classifier} | {message}") { }
}
