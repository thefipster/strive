using TheFipster.ActivityAggregator.Domain.Enums;

namespace TheFipster.ActivityAggregator.Domain.Models.Indexes;

public class BundleIndex
{
    public BundleIndex() { }

    public BundleIndex(
        int version,
        DateTime timestamp,
        DateRanges range,
        DataKind kind,
        List<string> extractions
    )
    {
        Version = version;
        Timestamp = timestamp;
        Range = range;
        Kind = kind;
        Extractions = extractions;

        Id = $"{timestamp:yyyy-MM-dd HH:mm:ss}_{kind}";
    }

    public int Version { get; set; }
    public DateTime IndexedAt { get; set; } = DateTime.UtcNow;
    public DateRanges Range { get; set; }
    public DataKind Kind { get; set; }
    public DateTime Timestamp { get; set; }
    public List<string> Extractions { get; set; } = new();
    public string? Id { get; set; }
}
