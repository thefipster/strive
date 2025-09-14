using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Domain.Models;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Importer.Modules.Abstractions;
using TheFipster.ActivityAggregator.Pipeline.Abstractions;
using TheFipster.ActivityAggregator.Pipeline.Config;
using TheFipster.ActivityAggregator.Pipeline.Models;
using TheFipster.ActivityAggregator.Pipeline.Models.Events;
using TheFipster.ActivityAggregator.Storage.Abstractions;

namespace TheFipster.ActivityAggregator.Pipeline.Stages;

public class TransformerStage(
    IOptions<ExtractorConfig> options,
    PipelineState state,
    IImporterRegistry registry,
    ITransformIndexer indexer
) : ITransfomerStage
{
    public const int Version = 1;
    public const string Name = "transformer";

    private readonly ExtractorConfig config = options.Value;
    private readonly IEnumerable<IFileExtractor> extractors = registry.LoadExtractors();

    private readonly ConcurrentQueue<ClassificationIndex> queue = new();
    private CancellationToken token;
    private readonly ProgressCounters counters = new();

    public event EventHandler<ProgressReportEventArgs>? ReportProgress;
    public event EventHandler<ErrorReportEventArgs>? ReportError;
    public event EventHandler<ResultReportEventArgs<TransformIndex>>? ReportResult;

    public void Enqueue(ClassificationIndex classification)
    {
        queue.Enqueue(classification);
        counters.In.Increment();
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        token = cancellationToken;
        var tasks = new List<Task> { CreateTransformTask(), CreateReportTask() };
        await Task.WhenAll(tasks);
    }

    #region Transform

    private Task CreateTransformTask()
    {
        var extractionTask = Task.Run(
            async () =>
            {
                while (!token.IsCancellationRequested && JobsAvailable)
                {
                    if (queue.TryDequeue(out var input))
                    {
                        try
                        {
                            TryTransform(input);
                        }
                        catch (Exception e)
                        {
                            ReportError?.Invoke(this, new ErrorReportEventArgs(Name, e));
                        }
                    }
                    else
                    {
                        await Task.Delay(10, token);
                    }
                }

                state.FinishedStages.Add(Name);
            },
            token
        );

        return extractionTask;
    }

    private bool JobsAvailable =>
        !state.FinishedStages.Contains(ClassifierStage.Name) || !queue.IsEmpty;

    private void TryTransform(ClassificationIndex input)
    {
        if (input.Classifications.Count() == 1)
            Transform(input);
        else
            counters.Skip.Increment();
    }

    private void Transform(ClassificationIndex input)
    {
        var classification = input.Classifications.First();
        var extractor = extractors.FirstOrDefault(x => x.Source == classification.Source);

        if (extractor == null)
        {
            counters.Skip.Increment();
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
                var filepath = extraction.Write(config.OutputDir);

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

        counters.Done.Increment();
    }

    private void EmitResult(TransformIndex index)
    {
        ReportResult?.Invoke(this, new(index));
        counters.Out.Increment();
    }

    #endregion

    #region Progress

    private Task CreateReportTask()
    {
        var reportTask = Task.Run(
            async () =>
            {
                while (!token.IsCancellationRequested && !state.FinishedStages.Contains(Name))
                    await EmitProgress(1000);
            },
            token
        );

        return reportTask;
    }

    private async Task EmitProgress(int delayInMs)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(delayInMs), token);
        ReportProgress?.Invoke(
            this,
            new ProgressReportEventArgs(
                Name,
                0,
                counters.In.Value,
                counters.Done.Value,
                counters.Skip.Value,
                counters.Out.Value
            )
        );
    }

    #endregion
}
