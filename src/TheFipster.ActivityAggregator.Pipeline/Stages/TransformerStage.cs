using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Domain.Models;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Importer.Modules.Abstractions;
using TheFipster.ActivityAggregator.Pipeline.Abstractions;
using TheFipster.ActivityAggregator.Pipeline.Config;
using TheFipster.ActivityAggregator.Pipeline.Models;
using TheFipster.ActivityAggregator.Pipeline.Models.Events;
using TheFipster.ActivityAggregator.Pipeline.Pipelines;
using TheFipster.ActivityAggregator.Storage.Abstractions;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Pipeline.Stages;

public class TransformerStage(
    PipelineState<IngesterPipeline> state,
    IOptions<ExtractorConfig> config,
    IImporterRegistry registry,
    IIndexer<TransformIndex> indexer,
    ILogger<TransformerStage> logger
) : ITransfomerStage
{
    public int Version => 1;
    public int Order => 30;

    public ProgressCounters Counters { get; } = new();

    private readonly IEnumerable<IFileExtractor> extractors = registry.LoadExtractors();
    private readonly ConcurrentQueue<ClassificationIndex> queue = new();

    public event EventHandler<ResultReportEventArgs<TransformIndex>>? ReportResult;

    public void Enqueue(ClassificationIndex classification)
    {
        queue.Enqueue(classification);
        Counters.In.Increment();
    }

    public async Task ExecuteAsync(CancellationToken ct)
    {
        var tasks = new List<Task>();
        for (int i = 0; i < config.Value.MaxDegreeOfParallelism; i++)
            tasks.Add(CreateTransformTask(ct));

        await Task.WhenAll(tasks);
        state.FinishedStages.Add(GetType().Name);
    }

    private Task CreateTransformTask(CancellationToken ct) =>
        Task.Run(async () => await LoopAsync(ct), ct);

    private async Task LoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested && JobsAvailable)
            await TryDequeue(ct);
    }

    private bool JobsAvailable =>
        !state.FinishedStages.Contains(nameof(ClassifierStage)) || !queue.IsEmpty;

    private async Task TryDequeue(CancellationToken ct)
    {
        if (queue.TryDequeue(out var input))
        {
            try
            {
                TryTransform(input);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Transformation failed for file {File}.", input.Filepath);
            }

            Counters.Done.Increment();
        }
        else
        {
            await Task.Delay(10, ct);
        }
    }

    private void TryTransform(ClassificationIndex input)
    {
        if (input.Classifications.Count() == 1)
            Transform(input);
        else
            Counters.Skip.Increment();
    }

    private void Transform(ClassificationIndex input)
    {
        var classification = input.Classifications.First();
        var extractor = extractors.FirstOrDefault(x => x.Source == classification.Source);

        if (extractor == null)
        {
            Counters.Skip.Increment();
            return;
        }

        var extractions = extractor.Extract(
            new ArchiveIndex
            {
                Filepath = input.Filepath,
                Source = extractor.Source,
                Date = classification.Datetime,
                Range = classification.Range,
            }
        );

        Parallel.ForEach(
            extractions,
            extraction =>
            {
                var filepath = extraction.Write(config.Value.OutputDirectory);

                var index = new TransformIndex(
                    Version,
                    filepath,
                    extraction.SourceFile,
                    extraction.Source,
                    extraction.Timestamp,
                    extraction.Range
                );

                indexer.Set(index);
                EmitResult(index);
            }
        );
    }

    private void EmitResult(TransformIndex index)
    {
        ReportResult?.Invoke(this, new(index));
        Counters.Out.Increment();
    }
}
