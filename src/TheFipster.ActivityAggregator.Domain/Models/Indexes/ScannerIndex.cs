using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Extraction;

namespace TheFipster.ActivityAggregator.Domain.Models.Indexes;

public class ScannerIndex
{
    public required string Hash { get; set; }

    public required string OriginHash { get; set; }

    public DateTime IndexedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsExcluded { get; set; }
    public ScannerRejections? ExclusionReason { get; set; }
    public Classification? Classification { get; set; }
    public List<Classification> Conflicts { get; set; } = new();
    public List<string> Files { get; set; } = new();
    public Dictionary<DateTime, ScannerActions> Actions { get; set; } = new();

    public override string ToString()
    {
        var lastAction = Actions.Any()
            ? Actions.OrderByDescending(x => x.Key).First().Value.ToString()
            : "No Actions";
        var classification = Classification == null ? "Not classifier" : Classification.ToString();

        return $"Scanner | {IndexedAt:s} | {string.Join(", ", Files)} | {classification} | {lastAction}";
    }
}
