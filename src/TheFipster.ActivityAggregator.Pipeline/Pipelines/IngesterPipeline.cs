using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Pipeline.Abstractions;
using TheFipster.ActivityAggregator.Pipeline.Config;
using TheFipster.ActivityAggregator.Pipeline.Models;

namespace TheFipster.ActivityAggregator.Pipeline.Pipelines;

public class IngesterPipeline(
    PipelineState<IngesterPipeline> state,
    IOptions<PipelineConfig> config,
    IOptions<ImporterConfig> import,
    IImporterStage importer,
    IScannerStage scanner,
    IClassifierStage classifier,
    ITransfomerStage transformer
) : IIngesterPipeline
{
    public event EventHandler<ProgressReportEventArgs>? ReportProgress;
    public bool IsFinished => state.FinishedStages.Count == stages.Count;

    private readonly IList<IStage> stages = [importer, scanner, classifier, transformer];

    public async Task ExecuteAsync(CancellationToken ct)
    {
        // plumbing
        importer.ReportResult += (_, args) => scanner.Enqueue(args.Result);
        scanner.ReportResult += (_, args) => classifier.Enqueue(args.Result);
        classifier.ReportResult += (_, args) => transformer.Enqueue(args.Result);

        // filling
        foreach (var input in import.Value.ImportDirectories)
            importer.Enqueue(input);

        // pumping
        var tasks = new List<Task>
        {
            importer.ExecuteAsync(ct),
            scanner.ExecuteAsync(ct),
            classifier.ExecuteAsync(ct),
            transformer.ExecuteAsync(ct),
        };

        if (!config.Value.IsSilent)
            tasks.Add(CreateReportTask(stages, ct));

        await Task.WhenAll(tasks);

        // flushing
        if (!config.Value.IsSilent)
            await EmitProgress(100, stages, ct);
    }

    private Task CreateReportTask(IList<IStage> pipelineStages, CancellationToken ct)
    {
        var reportTask = Task.Run(
            async () =>
            {
                while (!ct.IsCancellationRequested && !IsFinished)
                    await EmitProgress(100, pipelineStages, ct);
            },
            ct
        );

        return reportTask;
    }

    private async Task EmitProgress(
        int delayInMs,
        IEnumerable<IStage> pipelineStages,
        CancellationToken ct
    )
    {
        await Task.Delay(TimeSpan.FromMilliseconds(delayInMs), ct);
        foreach (var stage in pipelineStages)
        {
            ReportProgress?.Invoke(
                this,
                new ProgressReportEventArgs(
                    stage.GetType().Name,
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
