using TheFipster.ActivityAggregator.Domain.Enums;

namespace TheFipster.ActivityAggregator.Domain.Exceptions;

public class ClassificationException : Exception
{
    public ClassificationException() { }

    public ClassificationException(string message)
        : base(message) { }

    public ClassificationException(string filepath, string message)
        : base($"{filepath} | Classification | {message}") { }

    public ClassificationException(string filepath, DataSources classifier, string message)
        : base($"{filepath} | Classification | {classifier} | {message}") { }
}
