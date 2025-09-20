using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using TheFipster.ActivityAggregator.Pipeline.Abstractions;
using TheFipster.ActivityAggregator.Pipeline.Models;

namespace TheFipster.ActivityAggregator.Cli;

public class Runner(IIngesterPipeline ingester, IMergerPipeline merger, ILogger<Runner> logger)
{
    private readonly ConcurrentDictionary<string, ProgressReportEventArgs> progressReports = new();
    private int? cursorLine;

    public async Task ExecuteAsync(CancellationToken ct)
    {
        logger.LogInformation("Application started.");

        await RunPipeline(ingester, ct);
        await RunPipeline(merger, ct);

        logger.LogInformation("Application finished.");
    }

    private async Task RunPipeline(IPipeline pipeline, CancellationToken ct)
    {
        logger.LogInformation("{Pipeline} started.", pipeline.GetType().Name);
        Console.WriteLine($"Executing {pipeline.GetType().Name}");
        Console.WriteLine();
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        pipeline.ReportProgress += (_, args) => progressReports[args.Stage] = args;
        await Task.WhenAll(CreateReportTask(pipeline, ct), pipeline.ExecuteAsync(ct));
        await PrintPipeline(0, ct);
        progressReports.Clear();
        cursorLine = null;

        stopwatch.Stop();
        Console.WriteLine();
        Console.WriteLine(
            $"Finished {pipeline.GetType().Name} in {stopwatch.ElapsedMilliseconds:N0} ms."
        );
        Console.WriteLine();
        logger.LogInformation("{Pipeline} finished.", pipeline.GetType().Name);
    }

    private Task CreateReportTask(IPipeline pipeline, CancellationToken ct) =>
        Task.Run(async () => await ReportLoop(pipeline, ct), ct);

    private async Task ReportLoop(IPipeline pipeline, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested && !pipeline.IsFinished)
            await PrintPipeline(100, ct);
    }

    private async Task PrintPipeline(int delayInMs, CancellationToken ct)
    {
        if (!cursorLine.HasValue)
            cursorLine = Console.GetCursorPosition().Top;
        else
            Console.SetCursorPosition(0, cursorLine.Value);

        foreach (var stage in progressReports.OrderBy(x => x.Value.Order))
            PrintStage(stage.Value);

        await Task.Delay(TimeSpan.FromMilliseconds(delayInMs), ct);
    }

    void PrintStage(ProgressReportEventArgs report)
    {
        Console.WriteLine(
            $"{report.Stage, -20} || "
                + $"In: {report.Total, -6} Done: {report.Processed, -6} Skip: {report.Errors, -6} || "
                + $"Out: {report.Emitted, -6}"
        );
    }
}
