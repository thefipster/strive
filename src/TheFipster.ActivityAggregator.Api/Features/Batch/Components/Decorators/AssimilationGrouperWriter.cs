using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Api.Setup.Configs;
using TheFipster.ActivityAggregator.Domain.Extensions;
using TheFipster.ActivityAggregator.Domain.Models.Files;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Api.Features.Batch.Components.Decorators;

public class AssimilationGrouperWriter(
    IAssimilationGrouper component,
    IOptions<ImportConfig> config,
    IIndexer<BatchIndex> indexer
) : IAssimilationGrouper
{
    public async Task<Dictionary<MergedFile, List<AssimilateIndex>>> CombinePerDayAsync(
        DateTime day,
        CancellationToken ct
    )
    {
        var mergeResults = await component.CombinePerDayAsync(day, ct);

        foreach (var result in mergeResults)
        {
            var merge = result.Key;
            var assimilations = result.Value;

            var path = merge.Write(config.Value.ConvergeDirectoryPath);
            var file = new FileInfo(path);
            var hash = await file.HashXx3Async(ct);

            List<string> parameters = ExtractParameters(merge);

            var index = BatchIndex.New(path, hash, merge, parameters, assimilations);
            indexer.Set(index);
        }

        return mergeResults;
    }

    private List<string> ExtractParameters(MergedFile merge)
    {
        var parameters = new List<string>();

        if (merge.Metrics != null)
        {
            var resolvedMetrics = merge.Metrics.Resolved.Keys.Select(x => x.ToString());
            parameters.AddRange(resolvedMetrics);

            var conflictedMetrics = merge.Metrics.Conflicts.Keys.Select(x => x.ToString());
            parameters.AddRange(conflictedMetrics);
        }

        var samples = merge.Samples.SelectMany(x => x.Values.Select(y => y.Key.ToString()));
        parameters.AddRange(samples);

        if (merge.Events != null)
        {
            var events = merge.Events.Resolved.Select(x => x.Type.ToString());
            parameters.AddRange(events);
        }

        return parameters;
    }
}
