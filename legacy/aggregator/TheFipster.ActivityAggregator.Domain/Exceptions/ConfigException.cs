namespace TheFipster.ActivityAggregator.Domain.Exceptions;

public class ConfigException : Exception
{
    public ConfigException() { }

    public ConfigException(string key, string message)
        : base($"{key}: {message}") { }
}
