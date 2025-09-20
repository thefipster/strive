using TheFipster.ActivityAggregator.Domain.Enums;

namespace TheFipster.ActivityAggregator.Domain.Models.Indexes;

public class UnifyIndex
{
    public UnifyIndex() { }

    public UnifyIndex(int version, DateTime timestamp, DataKind kind, bool conflicts)
    {
        Version = version;
        Timestamp = timestamp;
        Kind = kind;
        HasConflicts = conflicts;

        Id = $"{timestamp:yyyy-MM-dd HH:mm:ss}_{kind}";
        Day = timestamp.Date;
        Month = new DateTime(timestamp.Year, timestamp.Month, 1);
        Year = new DateTime(timestamp.Year, 1, 1);
    }

    public int Version { get; set; }
    public DateTime IndexedAt { get; set; } = DateTime.UtcNow;
    public DataKind Kind { get; set; }
    public DateTime Timestamp { get; set; }
    public bool HasConflicts { get; set; }

    public string? Id { get; set; }
    public DateTime Day { get; set; }
    public DateTime Month { get; set; }
    public DateTime Year { get; set; }
}
