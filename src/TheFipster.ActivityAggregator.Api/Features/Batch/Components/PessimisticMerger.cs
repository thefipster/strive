using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Domain.Configs;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Extensions;
using TheFipster.ActivityAggregator.Domain.Models.Files;
using TheFipster.ActivityAggregator.Domain.Models.Importing;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Api.Features.Batch.Components;

public class PessimisticMerger(
    IMetricsMerger metricsMerger,
    IEventsMerger eventsMerger,
    ISeriesNormalizer seriesMerger,
    IOptions<ApiConfig> config
) : IPessimisticMerger
{
    public async Task<BatchIndex> HandleAssimilationGroupAsync(
        DateTime timestamp,
        DataKind kind,
        List<AssimilateIndex> assimilations,
        CancellationToken ct
    )
    {
        ct.ThrowIfCancellationRequested();

        var files = assimilations.Select(x => x.Path).ToArray();
        var extracts = files.Select(FileExtraction.FromFile).ToArray();
        var parameters = new List<string>();

        var metrics = MergeMetrics(extracts, parameters);
        var events = MergeEvents(extracts, parameters);
        var samples = NormalizeSamples(extracts, parameters);

        var mergeFile = MergedFile.New(
            timestamp,
            kind,
            extracts,
            events,
            samples,
            metrics,
            assimilations
        );

        var filepath = mergeFile.Write(config.Value.MergeDirectoryPath);
        var file = new FileInfo(filepath);
        var hash = await file.HashXx3Async(ct);

        return BatchIndex.New(filepath, hash, mergeFile, parameters, assimilations);
    }

    private NormalizedResult[] NormalizeSamples(FileExtraction[] extracts, List<string> parameters)
    {
        var series = extracts.Select(x => x.Series).ToArray();
        var seriesMerge = series.Select(seriesMerger.Normalize).ToArray();
        var seriesParameters = series.SelectMany(x => x.Keys.Select(y => y.ToString()));

        parameters.AddRange(seriesParameters);

        return seriesMerge;
    }

    private EventMergeResult MergeEvents(FileExtraction[] extracts, List<string> parameters)
    {
        var events = extracts.Select(x => x.Events.ToArray()).ToArray();
        var eventMerge = eventsMerger.Merge(events);
        var eventParameters = events.SelectMany(x => x.Select(y => y.Type.ToString()));

        parameters.AddRange(eventParameters);

        return eventMerge;
    }

    private MetricMergeResult MergeMetrics(FileExtraction[] extracts, List<string> parameters)
    {
        var metrics = extracts.Select(x => x.Attributes).ToArray();
        var metricsMerge = metricsMerger.Merge(metrics);
        var metricsParameters = metrics.SelectMany(x => x.Keys.Select(y => y.ToString()));

        parameters.AddRange(metricsParameters);

        return metricsMerge;
    }
}
