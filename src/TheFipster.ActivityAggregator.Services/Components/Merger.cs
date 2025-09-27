using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Merging;
using TheFipster.ActivityAggregator.Services.Abstractions;

namespace TheFipster.ActivityAggregator.Services.Components;

public class Merger : IMerger
{
    public MergedRecord Combine(BundleIndex bundle)
    {
        var extractions = bundle.Extractions.Select(FileExtraction.FromFile).ToList();

        var result = new MergedRecord();

        var allMetrics = extractions.Select(x => x.Attributes).ToArray();
        var mergedMetrics = Merge(allMetrics);
        result.Metrics = mergedMetrics.Resolved;
        result.Conflicts = mergedMetrics.Conflicts;

        var allSeries = extractions.Select(x => x.Series).ToArray();
        var normalizedSeries = Normalize(allSeries);

        return result;
    }

    private Dictionary<SampleTypes, List<NormalizedSample>> Normalize(
        params Dictionary<Parameters, List<string>>[] allSeries
    )
    {
        var result = new Dictionary<SampleTypes, List<NormalizedSample>>();

        foreach (var series in allSeries) { }

        return result;
    }

    public MetricMergeResult Merge(params Dictionary<Parameters, string>[] metrics)
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

    private Dictionary<Parameters, HashSet<string>> CollectValues(
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
}
