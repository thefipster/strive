using TheFipster.ActivityAggregator.Domain.Enums;

namespace TheFipster.ActivityAggregator.Domain.Models.Indexes;

public class AssimilaterIndex
{
    public required string ValueHash { get; set; }
    public required string OriginHash { get; set; }
    public DataSources Source { get; set; }
    public DateTime Timestamp { get; set; }
    public DateRanges Range { get; set; }
    public DateTime IndexedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<DateTime, AssimilaterActions> Actions { get; set; } = new();
}
