using System.Diagnostics;
using Microsoft.AspNetCore.SignalR.Client;
using TheFipster.ActivityAggregator.Api.Abstraction;
using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Extensions;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Scanner;
using TheFipster.ActivityAggregator.Services.Abstractions;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Api.Services;

public class ScannerService : IScannerService
{
    private readonly HubConnection connection;
    private readonly IClassifier classifier;
    private readonly IScanner scanner;
    private readonly IIndexer<ZipIndex> zipInventory;
    private readonly IIndexer<FileIndex> fileInventory;

    public ScannerService(
        IScanner scanner,
        IClassifier classifier,
        IIndexer<ZipIndex> zipInventory,
        IIndexer<FileIndex> fileInventory
    )
    {
        this.scanner = scanner;
        this.classifier = classifier;
        this.zipInventory = zipInventory;
        this.fileInventory = fileInventory;

        connection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7260/hubs/ingest")
            .Build();
        connection.StartAsync().Wait();
    }

    public async Task CheckImportAsync(ImporterIndex import, CancellationToken ct)
    {
        var files = Directory
            .EnumerateFiles(import.Output, "*", SearchOption.AllDirectories)
            .ToArray();

        var watch = new Stopwatch();
        watch.Start();

        foreach (var file in files)
        {
            await scanner.CheckAsync(file, import.Hash, ct);

            if (watch.ElapsedMilliseconds > 1000)
            {
                watch.Restart();
            }
        }
        watch.Stop();
    }

    public async Task CheckDirectoryAsync(string destinationDirectory, CancellationToken ct)
    {
        var zips = zipInventory.GetAll().ToArray();

        var scanCounter = 0;
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        foreach (var zip in zips)
        {
            var dir = new DirectoryInfo(zip.OutputPath);
            await connection.InvokeAsync(
                Const.Hubs.Ingester.WorkerInfo,
                $"Starting file scan on {dir.Name}.",
                cancellationToken: ct
            );

            var files = Directory
                .EnumerateFiles(zip.OutputPath, "*", SearchOption.AllDirectories)
                .ToArray();

            foreach (var filepath in files)
            {
                var file = new FileInfo(filepath);
                var hash = await file.HashXx3Async(ct);

                var index = fileInventory.GetById(hash);
                if (index != null)
                {
                    if (!index.AlternateFiles.Contains(filepath))
                        index.AlternateFiles.Add(filepath);

                    fileInventory.Set(index);

                    if (index.Source.HasValue)
                        continue;
                }

                index = new FileIndex
                {
                    Hash = hash,
                    ZipHash = zip.Hash,
                    Size = file.Length,
                    Path = filepath,
                };

                fileInventory.Set(index);

                List<ClassificationResult> results;
                try
                {
                    results = classifier.Classify(file, ct);
                }
                catch (Exception)
                {
                    continue;
                }

                if (results.Count(x => x.Classification != null) != 1)
                    continue;

                var result = results.First(x => x.Classification != null);
                index.Source = result.Classification?.Source;
                index.Timestamp = result.Classification?.Datetime;
                index.Range = result.Classification?.Range;

                fileInventory.Set(index);

                scanCounter++;
                await ReportProgressAsync(stopwatch, scanCounter, ct);
            }
        }
        stopwatch.Stop();

        await connection.InvokeAsync(
            Const.Hubs.Ingester.FileScanFinished,
            "File scan finished.",
            cancellationToken: ct
        );

        await connection.InvokeAsync(
            Const.Hubs.Ingester.WorkerInfo,
            "File scan finished.",
            cancellationToken: ct
        );
    }

    private async Task ReportProgressAsync(
        Stopwatch stopwatch,
        int scanCounter,
        CancellationToken ct
    )
    {
        if (stopwatch.ElapsedMilliseconds < 5000)
            return;

        await connection.InvokeAsync(
            Const.Hubs.Ingester.FileScanProgress,
            scanCounter,
            cancellationToken: ct
        );

        stopwatch.Restart();
    }
}
