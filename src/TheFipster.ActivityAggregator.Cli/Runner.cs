using Microsoft.Extensions.Logging;
using TheFipster.ActivityAggregator.Pipeline.Abstractions;
using TheFipster.ActivityAggregator.Pipeline.Models;
using TheFipster.ActivityAggregator.Pipeline.Stages;

namespace TheFipster.ActivityAggregator.Cli;

public class Runner(IPipeline pipeline, PipelineState state, ILogger<Runner> logger)
{
    private Dictionary<string, ProgressReportEventArgs> progressReports = new();
    private int? cursorLine;

    public async Task ExecuteAsync(CancellationToken token = default)
    {
        pipeline.ReportProgress += (_, args) => progressReports[args.Stage] = args;
        pipeline.ReportError += (_, args) => logger.LogWarning(args.Message);

        var tasks = new List<Task>();

        tasks.Add(pipeline.ExecuteAsync(token));
        tasks.Add(CreateReportTask(token));

        await Task.WhenAll(tasks);

        // make sure everything shows 100% usually the reporting is cut short when the last message is processed.
        await EmitProgress(0, token);
    }

    private Task CreateReportTask(CancellationToken token = default)
    {
        var reportTask = Task.Run(async () =>
        {
            while (
                !token.IsCancellationRequested
                && (
                    !state.FinishedStages.Contains(ScannerStage.Name)
                    || !state.FinishedStages.Contains(ClassifierStage.Name)
                    || !state.FinishedStages.Contains(TransformerStage.Name)
                )
            )
                await EmitProgress(1000, token);
        });

        return reportTask;
    }

    private async Task EmitProgress(int delayInMs, CancellationToken token)
    {
        if (!cursorLine.HasValue)
            cursorLine = Console.GetCursorPosition().Top;
        else
            Console.SetCursorPosition(0, cursorLine.Value);

        foreach (var stage in progressReports.OrderBy(x => x.Key))
            DrawStageProgress(stage.Value);

        await Task.Delay(TimeSpan.FromMilliseconds(delayInMs), token);
    }

    void DrawStageProgress(ProgressReportEventArgs report, int barWidth = 20)
    {
        double progress =
            report.Total > 0 ? (double)(report.Processed + report.Errors) / report.Total : 0;

        int filled = (int)(progress * barWidth);

        Console.WriteLine(
            $"{report.Stage, -12} [{new string('#', filled)}{new string('-', barWidth - filled)}] {progress * 100, 6:0.0}% || "
                + $"Queue: {report.Queue, 6} || "
                + $"In: {report.Total, -6} Done: {report.Processed, -6} Skip: {report.Errors, -6} || "
                + $"Out: {report.Emitted, -6}"
        );
    }
}
