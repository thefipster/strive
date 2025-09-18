using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Pipeline.Abstractions;
using TheFipster.ActivityAggregator.Pipeline.Config;
using TheFipster.ActivityAggregator.Pipeline.Models;

namespace TheFipster.ActivityAggregator.Pipeline;

public class Pipeline(
    PipelineState state,
    IOptions<PipelineConfig> config,
    IScannerStage scanner,
    IClassifierStage classifier,
    ITransfomerStage transformer
) : IPipeline
{
    private CancellationToken token;

    private readonly IEnumerable<IStage> stages = [scanner, classifier, transformer];

    public event EventHandler<ProgressReportEventArgs>? ReportProgress;

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        token = cancellationToken;

        // plumbing
        scanner.ReportResult += (_, args) => classifier.Enqueue(args.Result);
        classifier.ReportResult += (_, args) => transformer.Enqueue(args.Result);

        // filling
        foreach (var input in config.Value.ImportDirectories)
            scanner.Enqueue(input);

        // running
        await Task.WhenAll(
            scanner.ExecuteAsync(token),
            classifier.ExecuteAsync(token),
            transformer.ExecuteAsync(token),
            CreateReportTask()
        );

        // flushing
        await EmitProgress(10);
    }

    private Task CreateReportTask()
    {
        var reportTask = Task.Run(
            async () =>
            {
                while (!token.IsCancellationRequested && AnyStageRunning)
                    await EmitProgress(100);
            },
            token
        );

        return reportTask;
    }

    private bool AnyStageRunning => state.FinishedStages.Count < stages.Count();

    private async Task EmitProgress(int delayInMs)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(delayInMs), token);
        foreach (var stage in stages)
        {
            ReportProgress?.Invoke(
                this,
                new ProgressReportEventArgs(
                    stage.Name,
                    stage.Order,
                    stage.Counters.In.Value,
                    stage.Counters.Done.Value,
                    stage.Counters.Skip.Value,
                    stage.Counters.Out.Value
                )
            );
        }
    }
}
