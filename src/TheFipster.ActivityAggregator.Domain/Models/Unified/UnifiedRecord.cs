using TheFipster.ActivityAggregator.Domain.Enums;

namespace TheFipster.ActivityAggregator.Domain.Models;

public class UnifiedRecord
{
    public UnifiedRecord() { }

    public UnifiedRecord(DateTime timestamp, DataKind kind)
    {
        Timestamp = timestamp;
        Kind = kind;
    }

    public UnifiedRecord(
        DateTime timestamp,
        DataKind kind,
        Dictionary<Parameters, string> metrics,
        Dictionary<Parameters, List<string>> conflicts
    )
        : this(timestamp, kind)
    {
        Metrics = metrics;
        Conflicts = conflicts;
    }

    public UnifiedRecord(
        DateTime timestamp,
        DataKind kind,
        Dictionary<Parameters, string> metrics,
        Dictionary<Parameters, List<string>> conflicts,
        List<UnifiedSamples> samples
    )
        : this(timestamp, kind, metrics, conflicts)
    {
        Samples = samples;
    }

    public UnifiedRecord(
        DateTime timestamp,
        DataKind kind,
        Dictionary<Parameters, string> metrics,
        Dictionary<Parameters, List<string>> conflicts,
        List<UnifiedEvent> events
    )
        : this(timestamp, kind, metrics, conflicts, new(), events) { }

    public UnifiedRecord(
        DateTime timestamp,
        DataKind kind,
        Dictionary<Parameters, string> metrics,
        Dictionary<Parameters, List<string>> conflicts,
        List<UnifiedSamples> samples,
        List<UnifiedEvent> events
    )
        : this(timestamp, kind, metrics, conflicts, samples)
    {
        Events = events;
    }

    public DateTime Timestamp { get; set; }
    public DataKind Kind { get; set; }
    public Dictionary<Parameters, string> Metrics { get; set; } = new();
    public Dictionary<Parameters, List<string>> Conflicts { get; set; } = new();
    public List<UnifiedSamples> Samples { get; set; } = new();
    public List<UnifiedEvent> Events { get; set; } = new();
}
