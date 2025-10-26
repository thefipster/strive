using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;

namespace Fip.Strive.Core.Domain.Schemas.Ingestion.Models;

public class UnifiedEvent
{
    public UnifiedEvent() { }

    public UnifiedEvent(EventTypes type, DateTime timestamp)
    {
        Type = type;
        Timestamp = timestamp;
    }

    public UnifiedEvent(EventTypes type, DateTime timestamp, string? message)
        : this(type, timestamp)
    {
        Message = message;
    }

    public UnifiedEvent(
        EventTypes type,
        DateTime timestamp,
        string message,
        Parameters parameter,
        string value
    )
        : this(type, timestamp, message)
    {
        Values.Add(parameter, value);
    }

    public EventTypes Type { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<Parameters, string> Values { get; set; } = new();
    public string? Message { get; set; }
}
