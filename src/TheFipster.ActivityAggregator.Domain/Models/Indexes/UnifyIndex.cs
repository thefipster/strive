namespace TheFipster.ActivityAggregator.Domain.Models.Indexes;

public class UnifyIndex(int version, DateTime timestamp, DateRanges range, DataKind kind)
{
    public int Version { get; set; } = version;
    public DateTime IndexedAt { get; set; } = DateTime.UtcNow;
    public DateRanges Range { get; set; } = range;
    public DataKind Kind { get; set; } = kind;
    public DateTime Timestamp { get; set; } = timestamp;
}
