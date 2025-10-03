namespace TheFipster.ActivityAggregator.Domain.Models;

public record MetricMergeResult(
    Dictionary<Parameters, string> Resolved,
    Dictionary<Parameters, List<string>> Conflicts
)
{
    public int Count => Resolved.Count + Conflicts.Count;
}
