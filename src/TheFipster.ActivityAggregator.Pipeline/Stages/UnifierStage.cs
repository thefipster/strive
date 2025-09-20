using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Domain.Models;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Pipeline.Abstractions;
using TheFipster.ActivityAggregator.Pipeline.Config;
using TheFipster.ActivityAggregator.Pipeline.Models;
using TheFipster.ActivityAggregator.Pipeline.Models.Events;
using TheFipster.ActivityAggregator.Pipeline.Pipelines;
using TheFipster.ActivityAggregator.Storage.Abstractions;
using TheFipster.ActivityAggregator.Storage.Abstractions.Activity;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Pipeline.Stages;

public class UnifierStage(
    PipelineState<MergerPipeline> state,
    IOptions<UnifierConfig> config,
    IIndexer<UnifyIndex> indexer,
    IUnifiedRecordWriter writer,
    ILogger<UnifierStage> logger
) : Stage<BundleIndex, UnifyIndex>, IUnifierStage
{
    public int Version => 1;
    public int Order => 20;
    public event EventHandler<ResultReportEventArgs<UnifyIndex>>? ReportResult;

    public async Task ExecuteAsync(CancellationToken ct)
    {
        var tasks = new List<Task>();
        for (int i = 0; i < config.Value.MaxDegreeOfParallelism; i++)
            tasks.Add(CreateUnifierTask(ct));

        await Task.WhenAll(tasks);
        state.FinishedStages.Add(GetType().Name);
    }

    private Task CreateUnifierTask(CancellationToken ct) =>
        Task.Run(async () => await LoopAsync(ct), ct);

    private async Task LoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested && JobsAvailable)
            await TryDequeue();
    }

    private bool JobsAvailable =>
        !state.FinishedStages.Contains(nameof(BundlerStage)) || !queue.IsEmpty;

    private async Task TryDequeue()
    {
        if (queue.TryDequeue(out var input))
        {
            try
            {
                await ProcessInput(input);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Bundling failed for input {Input}.", input);
            }

            Counters.Done.Increment();
        }
        else
        {
            await Task.Delay(10);
        }
    }

    private Task ProcessInput(BundleIndex bundle)
    {
        var fragments = new List<FileExtraction>();
        foreach (var file in bundle.Extractions)
        {
            var extraction = FileExtraction.FromFile(file);
            fragments.Add(extraction);
        }

        var allMetrics = fragments.Select(x => x.Attributes).ToArray();
        var mergeResult = Merger.Merge(allMetrics);

        var unifiedRecord = new UnifiedRecord(
            bundle.Timestamp,
            bundle.Kind,
            mergeResult.Resolved,
            mergeResult.Conflicts
        );
        writer.Upsert(unifiedRecord);

        var unifiedIndex = new UnifyIndex(
            Version,
            bundle.Timestamp,
            bundle.Kind,
            unifiedRecord.Conflicts.Any()
        );
        indexer.Set(unifiedIndex);

        ReportResult?.Invoke(this, new(unifiedIndex));
        Counters.Out.Increment();
        return Task.CompletedTask;
    }
}
