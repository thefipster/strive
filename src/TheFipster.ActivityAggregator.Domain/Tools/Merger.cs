using TheFipster.ActivityAggregator.Domain.Models;

namespace TheFipster.ActivityAggregator.Domain.Tools;

public class Merger
{
    public static MetricMergeResult Merge(params Dictionary<Parameters, string>[] metrics)
    {
        var valueMap = CollectValues(metrics);
        var resolved = new Dictionary<Parameters, string>();
        var conflicts = new Dictionary<Parameters, List<string>>();

        foreach (var kvp in valueMap)
            if (kvp.Value.Count == 1)
                resolved[kvp.Key] = kvp.Value.First();
            else
                conflicts[kvp.Key] = kvp.Value.ToList();

        return new MetricMergeResult(resolved, conflicts);
    }

    private static Dictionary<Parameters, HashSet<string>> CollectValues(
        Dictionary<Parameters, string>[] dictionaries
    )
    {
        var valueMap = new Dictionary<Parameters, HashSet<string>>();

        foreach (var dict in dictionaries)
        {
            foreach (var kvp in dict)
            {
                if (!valueMap.TryGetValue(kvp.Key, out var set))
                {
                    set = new HashSet<string>();
                    valueMap[kvp.Key] = set;
                }

                set.Add(kvp.Value);
            }
        }

        return valueMap;
    }

    public static object Merge(params Dictionary<Parameters, List<string>>[] series)
    {
        throw new NotImplementedException();
    }
}
