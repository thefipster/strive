using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using TheFipster.ActivityAggregator.Pipeline.Abstractions;
using TheFipster.ActivityAggregator.Pipeline.Models;
using TheFipster.ActivityAggregator.Pipeline.Stages;

namespace TheFipster.ActivityAggregator.Cli;

public class Runner(IPipeline pipeline, PipelineState state, ILogger<Runner> logger)
{
    private readonly ConcurrentDictionary<string, ProgressReportEventArgs> progressReports = new();
    private int? cursorLine;

    public async Task ExecuteAsync(CancellationToken token = default)
    {
        logger.LogInformation("Pipeline started.");

        pipeline.ReportProgress += (_, args) => progressReports[args.Stage] = args;

        var tasks = new List<Task>();

        tasks.Add(pipeline.ExecuteAsync(token));
        tasks.Add(CreateReportTask(token));

        await Task.WhenAll(tasks);

        // make sure everything shows 100% usually the reporting is cut short when the last message is processed.
        await EmitProgress(0, token);

        logger.LogInformation("Pipeline finished.");
    }

    private Task CreateReportTask(CancellationToken ct)
    {
        var reportTask = Task.Run(
            async () =>
            {
                while (
                    !ct.IsCancellationRequested
                    && (
                        !state.FinishedStages.Contains(ScannerStage.Id)
                        || !state.FinishedStages.Contains(ClassifierStage.Id)
                        || !state.FinishedStages.Contains(TransformerStage.Id)
                    )
                )
                    await EmitProgress(100, ct);
            },
            ct
        );

        return reportTask;
    }

    private async Task EmitProgress(int delayInMs, CancellationToken ct)
    {
        if (!cursorLine.HasValue)
            cursorLine = Console.GetCursorPosition().Top;
        else
            Console.SetCursorPosition(0, cursorLine.Value);

        foreach (var stage in progressReports.OrderBy(x => x.Value.Order))
            DrawStageProgress(stage.Value);

        await Task.Delay(TimeSpan.FromMilliseconds(delayInMs), ct);
    }

    void DrawStageProgress(ProgressReportEventArgs report)
    {
        Console.WriteLine(
            $"{report.Stage, -12} || "
                + $"In: {report.Total, -6} Done: {report.Processed, -6} Skip: {report.Errors, -6} || "
                + $"Out: {report.Emitted, -6}"
        );
    }
}
