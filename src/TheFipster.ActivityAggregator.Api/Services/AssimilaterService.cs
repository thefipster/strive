using System.Diagnostics;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Api.Abstraction;
using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Configs;
using TheFipster.ActivityAggregator.Domain.Extensions;
using TheFipster.ActivityAggregator.Domain.Models;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Importer.Modules.Abstractions;
using TheFipster.ActivityAggregator.Services.Abstractions;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Api.Services;

public class AssimilaterService : IAssimilaterService
{
    private readonly IAssimilater assimilater;
    private readonly IPagedIndexer<FileIndex> fileInventory;
    private readonly IIndexer<ScannerIndex> indexer;
    private readonly IIndexer<ExtractorIndex> extractInventory;
    private readonly HubConnection connection;
    private readonly IImporterRegistry registry;
    private readonly IOptions<ApiConfig> config;
    private readonly IInventoryIndexer inventory;

    public AssimilaterService(
        IOptions<ApiConfig> config,
        IAssimilater assimilater,
        IPagedIndexer<FileIndex> fileInventory,
        IIndexer<ScannerIndex> indexer,
        IIndexer<ExtractorIndex> extractInventory,
        IInventoryIndexer inventory,
        IImporterRegistry registry
    )
    {
        this.assimilater = assimilater;
        this.fileInventory = fileInventory;
        this.extractInventory = extractInventory;
        this.indexer = indexer;
        this.registry = registry;
        this.config = config;
        this.inventory = inventory;

        connection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7260/hubs/ingest")
            .Build();
        connection.StartAsync().Wait();
    }

    public async Task ExtractFilesAsync(string destinationDirectory, CancellationToken ct)
    {
        await connection.InvokeAsync(
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
            var page = fileInventory.GetPaged(pageNo, 100);
            currentPageSize = page.Items.Count();
            pageNo++;

            foreach (var file in page.Items.Where(x => x.Source is not null))
            {
                if (ct.IsCancellationRequested)
                    return;

                var extractor = registry
                    .LoadExtractors()
                    .FirstOrDefault(x => x.Source == file.Source);
                if (extractor == null)
                    continue;

                var archive = new ArchiveIndex
                {
                    Source = file.Source!.Value,
                    Date = file.Timestamp!.Value,
                    Filepath = file.Path,
                    Range = file.Range!.Value,
                    Md5Hash = file.Hash,
                };

                try
                {
                    var result = extractor.Extract(archive);
                    var valueHashes = result.Select(x => x.ToHash());
                    var valueHash = valueHashes.ToUnorderedCollectionHash();
                    var extracts = new Dictionary<string, string>();
                    long size = 0;
                    foreach (var extract in result)
                    {
                        var extractFilepath = extract.Write(config.Value.ConvergeDirectoryPath);
                        var extractFile = new FileInfo(extractFilepath);
                        var hash = await extractFile.HashXx3Async(ct);
                        size += extractFile.Length;
                        extracts.Add(hash, extractFilepath);

                        var entry = new InventoryIndex(extract.Timestamp, extract.Range);
                        inventory.EnsureIndex(entry);
                    }

                    var extractionIndex = new ExtractorIndex
                    {
                        Path = file.Path,
                        FileHash = file.Hash,
                        ValueHash = valueHash,
                        ZipHash = file.ZipHash,
                        Source = file.Source,
                        Range = file.Range,
                        Timestamp = file.Timestamp,
                        ExtractedFiles = extracts,
                        Size = size,
                    };

                    extractInventory.Set(extractionIndex);
                }
                catch (Exception)
                {
                    continue;
                }

                assimilationCounter++;
                await ReportProgressAsync(stopwatch, assimilationCounter, ct);
            }
        }

        await connection.InvokeAsync(
            Const.Hubs.Ingester.AssimilationFinished,
            "Assimilation finished.",
            cancellationToken: ct
        );
    }

    public async Task ConvergeImportAsync(ImporterIndex import, CancellationToken ct)
    {
        var indexes = indexer.GetFiltered(x =>
            x.OriginHash == import.Hash && x.Classification != null
        );

        var procCount = 0;
        var outCount = 0;
        var watch = new Stopwatch();
        watch.Start();

        foreach (var index in indexes)
        {
            var assimilation = await assimilater.StandardizeAsync(index, ct);

            procCount++;
            outCount += assimilation.Count;

            if (watch.ElapsedMilliseconds > 1000)
            {
                await connection.InvokeAsync(
                    "Progress",
                    import.Hash,
                    procCount,
                    outCount,
                    cancellationToken: ct
                );
                watch.Restart();
            }
        }
        watch.Stop();
        await connection.InvokeAsync("Finished", import.Hash, cancellationToken: ct);
    }

    private async Task ReportProgressAsync(Stopwatch stopwatch, int counter, CancellationToken ct)
    {
        if (stopwatch.ElapsedMilliseconds < 5000)
            return;

        await connection.InvokeAsync(
            Const.Hubs.Ingester.AssimilationProgress,
            counter,
            cancellationToken: ct
        );

        stopwatch.Restart();
    }
}
