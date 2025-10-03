using System.Diagnostics;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Api.Abstraction;
using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Configs;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Extensions;
using TheFipster.ActivityAggregator.Domain.Models;
using TheFipster.ActivityAggregator.Domain.Models.Components;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Merging;
using TheFipster.ActivityAggregator.Services.Abstractions;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Api.Services;

public class BatchService : IBatchService
{
    private readonly ApiConfig config;

    private readonly IPagedIndexer<ExtractorIndex> extractInventory;
    private readonly IPagedIndexer<BatchIndex> batchInventory;
    private readonly IInventoryIndexer dateInventory;

    private readonly HubConnection connection;
    private readonly ILogger<BatchService> logger;
    private Stopwatch stopwatch;

    private readonly IMetricsMerger metricsMerger;
    private readonly IEventsMerger eventsMerger;
    private readonly ISeriesMerger seriesMerger;

    public BatchService(
        IOptions<ApiConfig> config,
        IPagedIndexer<ExtractorIndex> extractInventory,
        IPagedIndexer<BatchIndex> batchInventory,
        IInventoryIndexer dateInventory,
        IMetricsMerger metricsMerger,
        ISeriesMerger seriesMerger,
        IEventsMerger eventsMerger,
        ILogger<BatchService> logger
    )
    {
        this.config = config.Value;
        this.extractInventory = extractInventory;
        this.batchInventory = batchInventory;
        this.dateInventory = dateInventory;
        this.metricsMerger = metricsMerger;
        this.seriesMerger = seriesMerger;
        this.eventsMerger = eventsMerger;
        this.logger = logger;

        connection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7260/hubs/ingest")
            .Build();
        connection.StartAsync().Wait();
    }

    public async Task CombineFilesAsync(string convergancePath, CancellationToken ct)
    {
        await connection.InvokeAsync(
            Const.Hubs.Ingester.WorkerInfo,
            "Starting batch.",
            cancellationToken: ct
        );

        var minYear = dateInventory.GetMinYear();
        var curYear = DateTime.UtcNow.Year;
        var runYear = minYear;

        stopwatch = new Stopwatch();
        while (runYear <= curYear && !ct.IsCancellationRequested)
        {
            var year = runYear;
            var items = extractInventory
                .GetFiltered(x => x.Timestamp.HasValue && x.Timestamp.Value.Year == year)
                .ToArray();

            var dayCol = items
                .SelectMany(x => x.ExtractedFiles)
                .Where(x => x.Range == DateRanges.Day)
                .ToArray();
            var dayGroup = dayCol.GroupBy(x => x.Timestamp.Date);
            foreach (var group in dayGroup)
                await HandleExtractionGroup(ct, group, DataKind.Day);

            var sessionCol = items
                .SelectMany(x => x.ExtractedFiles)
                .Where(x => x.Range == DateRanges.Time)
                .ToArray();
            var sessionGroup = sessionCol.GroupBy(x => x.Timestamp);
            foreach (var group in sessionGroup)
                await HandleExtractionGroup(ct, group, DataKind.Session);

            runYear++;
        }

        await connection.InvokeAsync(
            Const.Hubs.Ingester.WorkerInfo,
            "Finished batch.",
            cancellationToken: ct
        );

        await connection.InvokeAsync(
            Const.Hubs.Ingester.BatchFinished,
            "Finished batch.",
            cancellationToken: ct
        );
    }

    private async Task HandleExtractionGroup(
        CancellationToken ct,
        IGrouping<DateTime, ExtractionSnippet> group,
        DataKind kind
    )
    {
        ct.ThrowIfCancellationRequested();

        var timestamp = group.Key;
        var indexes = group.ToArray();
        var files = indexes.Select(x => x.Path).ToArray();
        var extracts = files.Select(FileExtraction.FromFile).ToArray();
        var parameters = new List<string>();

        var metrics = extracts.Select(x => x.Attributes).ToArray();
        var metricsMerge = metricsMerger.Merge(metrics);
        var metricsParameters = metrics.SelectMany(x => x.Keys.Select(y => y.ToString()));
        parameters.AddRange(metricsParameters);

        var series = extracts.Select(x => x.Series).ToArray();
        var seriesMerge = series.Select(seriesMerger.Normalize).ToArray();
        var seriesParameters = series.SelectMany(x => x.Keys.Select(y => y.ToString()));
        parameters.AddRange(seriesParameters);

        var events = extracts.Select(x => x.Events.ToArray()).ToArray();
        var eventMerge = eventsMerger.Merge(events);
        var eventParameters = events.SelectMany(x => x.Select(y => y.Type.ToString()));
        parameters.AddRange(eventParameters);

        var timedSeries = seriesMerge.Where(x => x.Samples != null).Select(x => x.Samples).ToList();
        var tracks = seriesMerge.Where(x => x.Track != null).Select(x => x.Track).ToList();
        var pulses = seriesMerge.Where(x => x.Pulses != null).Select(x => x.Pulses).ToList();

        var mergeFile = new MergedFile
        {
            Timestamp = timestamp,
            Kind = kind,
            Sources = extracts.Select(x => x.Source).ToList(),
            Events = eventMerge,
            Series = timedSeries,
            Tracks = tracks,
            Pulses = pulses,
            Metrics = metricsMerge,
            Extractions = indexes.DistinctBy(x => x.Hash).ToDictionary(x => x.Hash, y => y.Path),
        };

        var filepath = mergeFile.Write(config.MergeDirectoryPath);
        var file = new FileInfo(filepath);
        var hash = await file.HashXx3Async(ct);

        var batchIndex = new BatchIndex
        {
            Timestamp = mergeFile.Timestamp,
            Kind = mergeFile.Kind,
            Sources = mergeFile.Sources,
            Metrics = mergeFile.Metrics.Count,
            Series = mergeFile.Series.Count,
            Tracks = mergeFile.Tracks.Count,
            Pulses = mergeFile.Pulses.Count,
            Events = mergeFile.Events.Resolved.Count,
            Parameters = parameters.Distinct().ToList(),
            Filepath = filepath,
            Hash = hash,
        };

        batchInventory.Set(batchIndex);

        await ReportProgressAsync(stopwatch, 0, ct);
    }

    private async Task ReportProgressAsync(Stopwatch stopwatch, int counter, CancellationToken ct)
    {
        if (stopwatch.ElapsedMilliseconds < 5000)
            return;

        await connection.InvokeAsync(
            Const.Hubs.Ingester.BatchProgress,
            counter,
            cancellationToken: ct
        );

        stopwatch.Restart();
    }
}
