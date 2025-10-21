using TheFipster.ActivityAggregator.Domain.Enums;

namespace TheFipster.ActivityAggregator.Domain.Models.Importing;

public record MetricMergeResult(
    Dictionary<Parameters, string> Resolved,
    Dictionary<Parameters, List<string>> Conflicts
)
{
    public int Count => Resolved.Count + Conflicts.Count;
}
