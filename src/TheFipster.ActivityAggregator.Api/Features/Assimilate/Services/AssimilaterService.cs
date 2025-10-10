using System.Diagnostics;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Api.Services.Contracts;
using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Configs;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Extensions;
using TheFipster.ActivityAggregator.Domain.Models.Importing;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;
using TheFipster.ActivityAggregator.Importer.Abstractions;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Api.Services;

public class AssimilaterService : IAssimilaterService
{
    private readonly IPagedIndexer<FileIndex> _fileInventory;
    private readonly IIndexer<ExtractorIndex> _extractInventory;
    private readonly HubConnection _connection;
    private readonly IImporterRegistry _registry;
    private readonly IOptions<ApiConfig> _config;
    private readonly IInventoryIndexer _inventory;
    private readonly IIndexer<AssimilateIndex> _assimilateInventory;

    public AssimilaterService(
        IOptions<ApiConfig> config,
        IPagedIndexer<FileIndex> fileInventory,
        IIndexer<ExtractorIndex> extractInventory,
        IIndexer<AssimilateIndex> assimilateInventory,
        IInventoryIndexer inventory,
        IImporterRegistry registry
    )
    {
        _fileInventory = fileInventory;
        _extractInventory = extractInventory;
        _assimilateInventory = assimilateInventory;
        _registry = registry;
        _config = config;
        _inventory = inventory;

        _connection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7260/hubs/ingest")
            .Build();
        _connection.StartAsync().Wait();
    }

    public async Task ExtractFilesAsync(string destinationDirectory, CancellationToken ct)
    {
        await _connection.InvokeAsync(
            Const.Hubs.Ingester.WorkerInfo,
            "Starting assimilation.",
            cancellationToken: ct
        );

        var pageNo = 0;
        var currentPageSize = int.MaxValue;

        var assimilationCounter = 0;
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        while (!ct.IsCancellationRequested && currentPageSize > 0)
        {
            var page = _fileInventory.GetPaged(pageNo, 100);
            currentPageSize = page.Items.Count();
            pageNo++;

            foreach (var file in page.Items.Where(x => x.Source is not null))
            {
                if (ct.IsCancellationRequested)
                    return;

                var extractionIndex = _extractInventory.GetById(file.Hash);
                if (extractionIndex != null)
                    continue;

                var extractor = _registry
                    .LoadExtractors()
                    .FirstOrDefault(x => x.Source == file.Source);
                if (extractor == null)
                    continue;

                var archive = new ExtractionRequest
                {
                    Source = file.Source!.Value,
                    Date = file.Timestamp!.Value,
                    Filepath = file.Path,
                    Range = file.Range!.Value,
                };

                try
                {
                    var result = extractor.Extract(archive);
                    var valueHashes = result.Select(x => x.ToHash());
                    var valueHash = valueHashes.ToUnorderedCollectionHash();
                    var extracts = new List<ExtractionSnippet>();
                    var metrics = new List<string>();
                    long size = 0;
                    foreach (var extract in result)
                    {
                        var extractFilepath = extract.Write(_config.Value.ConvergeDirectoryPath);
                        var extractFile = new FileInfo(extractFilepath);
                        var hash = await extractFile.HashXx3Async(ct);
                        size += extractFile.Length;

                        var snippet = new ExtractionSnippet
                        {
                            Timestamp = extract.Timestamp,
                            Path = extractFilepath,
                            Range = extract.Range,
                            Hash = hash,
                            Size = size,
                        };
                        extracts.Add(snippet);

                        metrics.AddRange(extract.Attributes.Keys.Select(x => x.ToString()));
                        metrics.AddRange(extract.Series.Keys.Select(x => x.ToString()));
                        metrics.AddRange(extract.Events.Select(x => x.Type.ToString()));

                        var entry = new InventoryIndex(extract.Timestamp, extract.Range);
                        _inventory.EnsureIndex(entry);
                    }

                    extractionIndex = new ExtractorIndex
                    {
                        Path = file.Path,
                        FileHash = file.Hash,
                        ValueHash = valueHash,
                        ZipHash = file.ZipHash,
                        Source = file.Source,
                        Range = file.Range,
                        Timestamp = file.Timestamp,
                        ExtractedFiles = extracts,
                        Metrics = metrics.Distinct().ToList(),
                        Size = size,
                    };

                    _extractInventory.Set(extractionIndex);

                    foreach (var extract in extracts)
                    {
                        var assimilation = new AssimilateIndex
                        {
                            Hash = extract.Hash,
                            FileHash = extractionIndex.FileHash,
                            Path = extract.Path,
                            Timestamp = extract.Timestamp,
                            Size = extract.Size,
                            Kind =
                                extract.Range == DateRanges.Time ? DataKind.Session : DataKind.Day,
                            Source = extractionIndex.Source.Value,
                        };

                        _assimilateInventory.Set(assimilation);
                    }
                }
                catch (Exception)
                {
                    continue;
                }

                assimilationCounter++;
                await ReportProgressAsync(stopwatch, assimilationCounter, ct);
            }
        }

        await _connection.InvokeAsync(
            Const.Hubs.Ingester.AssimilationFinished,
            "Assimilation finished.",
            cancellationToken: ct
        );

        await _connection.InvokeAsync(
            Const.Hubs.Ingester.WorkerInfo,
            "Assimilation finished.",
            cancellationToken: ct
        );
    }

    private async Task ReportProgressAsync(Stopwatch stopwatch, int counter, CancellationToken ct)
    {
        if (stopwatch.ElapsedMilliseconds < 5000)
            return;

        await _connection.InvokeAsync(
            Const.Hubs.Ingester.AssimilationProgress,
            counter,
            cancellationToken: ct
        );

        stopwatch.Restart();
    }
}
