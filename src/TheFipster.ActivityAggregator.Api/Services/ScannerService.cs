using System.Diagnostics;
using Microsoft.AspNetCore.SignalR.Client;
using Serilog;
using Serilog.Events;
using SerilogTracing;
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
    private readonly IIndexer<ZipIndex> zipInventory;
    private readonly IIndexer<FileIndex> fileInventory;
    private readonly ILogger<ScannerService> logger;

    public ScannerService(
        IClassifier classifier,
        IIndexer<ZipIndex> zipInventory,
        IIndexer<FileIndex> fileInventory,
        ILogger<ScannerService> logger
    )
    {
        this.classifier = classifier;
        this.zipInventory = zipInventory;
        this.fileInventory = fileInventory;
        this.logger = logger;

        connection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7260/hubs/ingest")
            .Build();
        connection.StartAsync().Wait();
    }

    public async Task CheckDirectoryAsync(string destinationDirectory, CancellationToken ct)
    {
        logger.LogInformation("Scanning directory {DestinationDirectory}", destinationDirectory);
        var scanActivity = Log.Logger.StartActivity("Scanning");
        var zips = zipInventory.GetAll().ToArray();

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
                using var activity = Log.Logger.StartActivity("Scan {File}", filepath);
                try
                {
                    await HandleFile(ct, filepath, zip, stopwatch);
                    activity.Complete();
                }
                catch (Exception e)
                {
                    activity.Complete(LogEventLevel.Fatal, e);
                }
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

        logger.LogInformation(
            "Scanning finished for directory {DestinationDirectory}",
            destinationDirectory
        );

        scanActivity.Complete();
    }

    private async Task HandleFile(
        CancellationToken ct,
        string filepath,
        ZipIndex zip,
        Stopwatch stopwatch
    )
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
                return;
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
            return;
        }

        if (results.Count(x => x.Classification != null) != 1)
            return;

        var result = results.First(x => x.Classification != null);
        index.Source = result.Classification?.Source;
        index.Timestamp = result.Classification?.Datetime;
        index.Range = result.Classification?.Range;

        fileInventory.Set(index);

        await ReportProgressAsync(stopwatch, ct);
    }

    private async Task ReportProgressAsync(Stopwatch stopwatch, CancellationToken ct)
    {
        if (stopwatch.ElapsedMilliseconds < 5000)
            return;

        await connection.InvokeAsync(
            Const.Hubs.Ingester.FileScanProgress,
            0,
            cancellationToken: ct
        );

        stopwatch.Restart();
    }
}
