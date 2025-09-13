namespace Fipster.TrackMe.Domain.Models;

public class UnifiedRecord
{
    public UnifiedRecord() { }

    public UnifiedRecord(DateTime timestamp)
    {
        Timestamp = timestamp;
    }

    public DateTime Timestamp { get; set; }

    public Dictionary<Parameters, string> Metrics { get; set; } = new();

    public Dictionary<Parameters, IEnumerable<string>> Series { get; set; } = new();
}
