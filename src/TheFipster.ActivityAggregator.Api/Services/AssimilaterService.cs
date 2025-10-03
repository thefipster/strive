using System.Diagnostics;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Api.Abstraction;
using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Configs;
using TheFipster.ActivityAggregator.Domain.Extensions;
using TheFipster.ActivityAggregator.Domain.Models;
using TheFipster.ActivityAggregator.Domain.Models.Components;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Importer.Modules.Abstractions;
using TheFipster.ActivityAggregator.Services.Abstractions;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Api.Services;

public class AssimilaterService : IAssimilaterService
{
    private readonly IPagedIndexer<FileIndex> fileInventory;
    private readonly IIndexer<ExtractorIndex> extractInventory;
    private readonly HubConnection connection;
    private readonly IImporterRegistry registry;
    private readonly IOptions<ApiConfig> config;
    private readonly IInventoryIndexer inventory;

    public AssimilaterService(
        IOptions<ApiConfig> config,
        IPagedIndexer<FileIndex> fileInventory,
        IIndexer<ExtractorIndex> extractInventory,
        IInventoryIndexer inventory,
        IImporterRegistry registry
    )
    {
        this.fileInventory = fileInventory;
        this.extractInventory = extractInventory;
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

                var extractionIndex = extractInventory.GetById(file.Hash);
                if (extractionIndex != null)
                    continue;

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
                    var extracts = new List<ExtractionSnippet>();
                    var metrics = new List<string>();
                    long size = 0;
                    foreach (var extract in result)
                    {
                        var extractFilepath = extract.Write(config.Value.ConvergeDirectoryPath);
                        var extractFile = new FileInfo(extractFilepath);
                        var hash = await extractFile.HashXx3Async(ct);
                        size += extractFile.Length;

                        var snippet = new ExtractionSnippet
                        {
                            Timestamp = extract.Timestamp,
                            Path = extractFilepath,
                            Range = extract.Range,
                            Hash = hash,
                        };
                        extracts.Add(snippet);

                        metrics.AddRange(extract.Attributes.Keys.Select(x => x.ToString()));
                        metrics.AddRange(extract.Series.Keys.Select(x => x.ToString()));
                        metrics.AddRange(extract.Events.Select(x => x.Type.ToString()));

                        var entry = new InventoryIndex(extract.Timestamp, extract.Range);
                        inventory.EnsureIndex(entry);
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

        await connection.InvokeAsync(
            Const.Hubs.Ingester.WorkerInfo,
            "Assimilation finished.",
            cancellationToken: ct
        );
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
