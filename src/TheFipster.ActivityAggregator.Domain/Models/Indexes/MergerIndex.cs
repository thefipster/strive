using TheFipster.ActivityAggregator.Domain.Enums;

namespace TheFipster.ActivityAggregator.Domain.Models.Indexes;

public class MergerIndex
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime IndexedAt { get; set; } = DateTime.UtcNow;
    public DateRanges Range { get; set; }
    public DataKind Kind { get; set; }
    public DateTime Timestamp { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public int Day { get; set; }
    public int Time { get; set; }
    public List<string> Extractions { get; set; } = new();
}
