using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Pipeline.Abstractions;
using TheFipster.ActivityAggregator.Pipeline.Config;
using TheFipster.ActivityAggregator.Pipeline.Models;

namespace TheFipster.ActivityAggregator.Pipeline.Pipelines;

public class MergerPipeline(
    PipelineState<MergerPipeline> state,
    IOptions<PipelineConfig> config,
    IOptions<BundlerConfig> bundle,
    IBundlerStage bundler,
    IUnifierStage unifier
) : IMergerPipeline
{
    public event EventHandler<ProgressReportEventArgs>? ReportProgress;

    private readonly IList<IStage> stages = [bundler, unifier];

    public bool IsFinished => state.FinishedStages.Count == stages.Count;

    public async Task ExecuteAsync(CancellationToken ct)
    {
        // plumbing
        bundler.ReportResult += (_, args) => unifier.Enqueue(args.Result);

        // filling
        bundler.Enqueue(bundle.Value.ImportDirectory);

        // pumping
        var tasks = new List<Task> { bundler.ExecuteAsync(ct), unifier.ExecuteAsync(ct) };

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
