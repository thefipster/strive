namespace TheFipster.ActivityAggregator.Domain.Exceptions;

public class IndexingException : Exception
{
    public IndexingException(string indexer, string message)
        : base($"{indexer}: {message}") { }

    public IndexingException(string indexer, string message, Exception exception)
        : base($"{indexer}: {message}", exception) { }
}
