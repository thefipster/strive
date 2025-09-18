namespace TheFipster.ActivityAggregator.Domain.Models.Indexes;

public class BundleIndex(
    int version,
    DateTime timestamp,
    DateRanges range,
    DataKind kind,
    List<string> extractions
)
{
    public int Version { get; set; } = version;
    public DateTime IndexedAt { get; set; } = DateTime.UtcNow;
    public DateRanges Range { get; set; } = range;
    public DataKind Kind { get; set; } = kind;
    public DateTime Timestamp { get; set; } = timestamp;
    public List<string> Extractions { get; set; } = extractions;
}
