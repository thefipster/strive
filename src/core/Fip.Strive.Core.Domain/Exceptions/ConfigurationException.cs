namespace Fip.Strive.Core.Domain.Exceptions;

public class ConfigurationException : Exception
{
    public ConfigurationException(string key, string message)
        : base($"{key}: {message}") { }

    public ConfigurationException(string section, string key, string message)
        : base($"{section.Replace(":", ".")}.{key}: {message}") { }

    public ConfigurationException(string section, string key, string message, Exception exception)
        : base($"{section.Replace(":", ".")}.{key}: {message}", exception) { }
}
