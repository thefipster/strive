using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Files;
using TheFipster.ActivityAggregator.Domain.Models.Importing;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Api.Features.Batch.Components;

public class PessimisticMerger(
    IMetricsMerger metricsMerger,
    IEventsMerger eventsMerger,
    ISeriesNormalizer seriesMerger
) : IPessimisticMerger
{
    public MergedFile CombineAssimilationGroup(
        DateTime timestamp,
        DataKind kind,
        List<AssimilateIndex> assimilations,
        CancellationToken ct
    )
    {
        ct.ThrowIfCancellationRequested();

        var files = assimilations.Select(x => x.Path).ToArray();
        var extracts = files.Select(FileExtraction.FromFile).ToArray();

        var metrics = MergeMetrics(extracts);
        var events = MergeEvents(extracts);
        var samples = NormalizeSamples(extracts);

        var mergeFile = MergedFile.New(
            timestamp,
            kind,
            extracts,
            events,
            samples,
            metrics,
            assimilations
        );

        return mergeFile;
    }

    private NormalizedResult[] NormalizeSamples(FileExtraction[] extracts)
    {
        var series = extracts.Select(x => x.Series).ToArray();
        return series.Select(seriesMerger.Normalize).ToArray();
    }

    private EventMergeResult MergeEvents(FileExtraction[] extracts)
    {
        var events = extracts.Select(x => x.Events.ToArray()).ToArray();
        return eventsMerger.Merge(events);
    }

    private MetricMergeResult MergeMetrics(FileExtraction[] extracts)
    {
        var metrics = extracts.Select(x => x.Attributes).ToArray();
        return metricsMerger.Merge(metrics);
    }
}
