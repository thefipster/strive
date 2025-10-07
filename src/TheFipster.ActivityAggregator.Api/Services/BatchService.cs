using System.Diagnostics;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Api.Abstraction;
using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Configs;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Extensions;
using TheFipster.ActivityAggregator.Domain.Models.Files;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Api.Services;

public class BatchService : IBatchService
{
    private readonly ApiConfig _config;

    private readonly IPagedIndexer<BatchIndex> _batchInventory;
    private readonly IInventoryIndexer _dateInventory;
    private readonly IPagedIndexer<AssimilateIndex> _assimilateInventory;

    private readonly HubConnection _connection;
    private Stopwatch _stopwatch = new();

    private readonly IMetricsMerger _metricsMerger;
    private readonly IEventsMerger _eventsMerger;
    private readonly ISeriesMerger _seriesMerger;

    public BatchService(
        IOptions<ApiConfig> config,
        IPagedIndexer<BatchIndex> batchInventory,
        IPagedIndexer<AssimilateIndex> assimilateInventory,
        IInventoryIndexer dateInventory,
        IMetricsMerger metricsMerger,
        ISeriesMerger seriesMerger,
        IEventsMerger eventsMerger
    )
    {
        _config = config.Value;
        _batchInventory = batchInventory;
        _assimilateInventory = assimilateInventory;
        _dateInventory = dateInventory;
        _metricsMerger = metricsMerger;
        _seriesMerger = seriesMerger;
        _eventsMerger = eventsMerger;

        _connection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7260/hubs/ingest")
            .Build();
        _connection.StartAsync().Wait();
    }

    public async Task CombineFilesAsync(string convergancePath, CancellationToken ct)
    {
        await ReportStarting(ct);

        var pageCount = int.MaxValue;
        var pageNo = 0;

        _stopwatch = new Stopwatch();
        while (pageCount > 0 && !ct.IsCancellationRequested)
        {
            var page = _dateInventory.GetDaysPaged(pageNo);
            pageNo++;
            pageCount = page.Items.Count();

            foreach (var item in page.Items)
                await HandleAssimilation(ct, item, item.IsDay ? DataKind.Day : DataKind.Session);
        }

        await ReportFinished(ct);
    }

    private async Task HandleAssimilation(CancellationToken ct, InventoryIndex item, DataKind kind)
    {
        var assimilations = _assimilateInventory
            .GetFiltered(x => x.Kind == kind && x.Timestamp == item.Timestamp)
            .ToList();

        await HandleExtractionGroup(ct, item.Timestamp, assimilations, kind);
    }

    private async Task HandleExtractionGroup(
        CancellationToken ct,
        DateTime timestamp,
        List<AssimilateIndex> assimilations,
        DataKind kind
    )
    {
        ct.ThrowIfCancellationRequested();

        var files = assimilations.Select(x => x.Path).ToArray();
        var extracts = files.Select(FileExtraction.FromFile).ToArray();
        var parameters = new List<string>();

        var metrics = extracts.Select(x => x.Attributes).ToArray();
        var metricsMerge = _metricsMerger.Merge(metrics);
        var metricsParameters = metrics.SelectMany(x => x.Keys.Select(y => y.ToString()));
        parameters.AddRange(metricsParameters);

        var events = extracts.Select(x => x.Events.ToArray()).ToArray();
        var eventMerge = _eventsMerger.Merge(events);
        var eventParameters = events.SelectMany(x => x.Select(y => y.Type.ToString()));
        parameters.AddRange(eventParameters);

        var series = extracts.Select(x => x.Series).ToArray();
        var seriesMerge = series.Select(_seriesMerger.Normalize).ToArray();
        var seriesParameters = series.SelectMany(x => x.Keys.Select(y => y.ToString()));
        parameters.AddRange(seriesParameters);

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
            Assimilations = assimilations
                .DistinctBy(x => x.Hash)
                .ToDictionary(x => x.Hash, y => y.Path),
        };

        var filepath = mergeFile.Write(_config.MergeDirectoryPath);
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
            Assimilations = assimilations
                .DistinctBy(x => x.Hash)
                .ToDictionary(x => x.Hash, y => y.Path),
            Filepath = filepath,
            Hash = hash,
        };

        _batchInventory.Set(batchIndex);

        await ReportProgressAsync(0, ct);
    }

    private async Task ReportStarting(CancellationToken ct)
    {
        await _connection.InvokeAsync(
            Const.Hubs.Ingester.WorkerInfo,
            "Starting batch.",
            cancellationToken: ct
        );
    }

    private async Task ReportProgressAsync(int counter, CancellationToken ct)
    {
        if (_stopwatch.ElapsedMilliseconds < 5000)
            return;

        await _connection.InvokeAsync(
            Const.Hubs.Ingester.BatchProgress,
            counter,
            cancellationToken: ct
        );

        _stopwatch.Restart();
    }

    private async Task ReportFinished(CancellationToken ct)
    {
        await _connection.InvokeAsync(
            Const.Hubs.Ingester.WorkerInfo,
            "Finished batch.",
            cancellationToken: ct
        );

        await _connection.InvokeAsync(
            Const.Hubs.Ingester.BatchFinished,
            "Finished batch.",
            cancellationToken: ct
        );
    }
}
