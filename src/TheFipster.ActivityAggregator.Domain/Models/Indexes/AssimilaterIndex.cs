using TheFipster.ActivityAggregator.Domain.Enums;

namespace TheFipster.ActivityAggregator.Domain.Models.Indexes;

public class AssimilaterIndex
{
    public required string OriginHash { get; set; }
    public required string FileHash { get; set; }
    public string? ValueHash { get; set; }
    public int Count { get; set; }
    public int ExtractorVersion { get; set; }
    public DataSources Source { get; set; }
    public DateTime IndexedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<DateTime, AssimilaterActions> Actions { get; set; } = new();
    public List<DateTime> Dates { get; set; } = [];
}
