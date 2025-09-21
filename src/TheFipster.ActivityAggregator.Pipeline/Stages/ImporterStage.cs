using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Pipeline.Abstractions;
using TheFipster.ActivityAggregator.Pipeline.Config;
using TheFipster.ActivityAggregator.Pipeline.Models;
using TheFipster.ActivityAggregator.Pipeline.Models.Events;
using TheFipster.ActivityAggregator.Pipeline.Pipelines;
using TheFipster.ActivityAggregator.Storage.Abstractions;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Pipeline.Stages;

public class ImporterStage(
    PipelineState<IngesterPipeline> state,
    IOptions<ImporterConfig> config,
    IIndexer<ImportIndex> indexer,
    ILogger<ImporterStage> logger
) : Stage<string, string>, IImporterStage
{
    public int Version => 1;
    public int Order => 5;
    public event EventHandler<ResultReportEventArgs<string>>? ReportResult;

    public async Task ExecuteAsync(CancellationToken ct)
    {
        await CreateImportTask(ct);
        state.FinishedStages.Add(GetType().Name);
    }

    private Task CreateImportTask(CancellationToken ct) =>
        Task.Run(async () => await LoopAsync(ct), ct);

    private async Task LoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested && !queue.IsEmpty)
            await TryDequeue(ct);
    }

    private async Task TryDequeue(CancellationToken ct)
    {
        if (queue.TryDequeue(out var input))
        {
            try
            {
                await ProcessInput(input, ct);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Importing failed for input {Input}.", input);
            }

            Counters.Done.Increment();
        }
        else
        {
            await Task.Delay(10, ct);
        }
    }

    private async Task ProcessInput(string directoryPath, CancellationToken ct)
    {
        await Task.Delay(100, ct);
        ReportResult?.Invoke(this, new(directoryPath));
        Counters.Out.Increment();
    }
}
