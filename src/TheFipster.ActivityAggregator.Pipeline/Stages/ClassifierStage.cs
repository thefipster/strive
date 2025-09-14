using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Importer.Modules.Abstractions;
using TheFipster.ActivityAggregator.Pipeline.Abstractions;
using TheFipster.ActivityAggregator.Pipeline.Models;
using TheFipster.ActivityAggregator.Pipeline.Models.Events;
using TheFipster.ActivityAggregator.Storage.Abstractions;

namespace TheFipster.ActivityAggregator.Pipeline.Stages;

public class ClassifierStage(
    PipelineState state,
    IImporterRegistry registry,
    IScanIndexer indexer,
    ILogger<ClassifierStage> logger
) : IClassifierStage
{
    public const int Version = 1;
    public const string Name = "classifier";

    private readonly IEnumerable<IFileClassifier> classifiers = registry.LoadClassifiers();

    private readonly ConcurrentQueue<ImportIndex> queue = new();
    private CancellationToken token;
    private readonly ProgressCounters counters = new();

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        token = cancellationToken;
        var tasks = new List<Task> { CreateReportTask(), CreateClassifyTask() };
        await Task.WhenAll(tasks);
    }

    public event EventHandler<ProgressReportEventArgs>? ReportProgress;
    public event EventHandler<ErrorReportEventArgs>? ReportError;
    public event EventHandler<ResultReportEventArgs<ClassificationIndex>>? ReportResult;

    public void Enqueue(ImportIndex import)
    {
        queue.Enqueue(import);
        counters.In.Increment();
    }

    #region Classification

    private Task CreateClassifyTask() =>
        Task.Run(
            async () =>
            {
                while (!token.IsCancellationRequested && JobsAvailable)
                {
                    if (queue.TryDequeue(out var import))
                    {
                        Process(import);
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

    private bool JobsAvailable =>
        !state.FinishedStages.Contains(ScannerStage.Name) || !queue.IsEmpty;

    private void Process(ImportIndex import)
    {
        var classifications = new ConcurrentBag<ImportClassification>();
        Parallel.ForEach(
            classifiers,
            classifier =>
            {
                try
                {
                    var result = classifier.Classify(import.Filepath);
                    if (result != null)
                        classifications.Add(result);
                }
                catch (Exception e)
                {
                    logger.LogError(
                        e,
                        "Classification failed for file {File} using {Classifier}.",
                        import.Filepath,
                        classifier.Source
                    );
                }
            }
        );

        var index = new ClassificationIndex(
            Version,
            import.Filepath,
            classifications.Select(result => result.ToClassification())
        );

        ReportClassificationResult(index);
    }

    private void ReportClassificationResult(ClassificationIndex index)
    {
        if (!index.Classifications.Any())
        {
            counters.Skip.Increment();
            logger.LogError("No classification found for file {File}.", index.Filepath);
        }

        if (index.Classifications.Count() > 1)
        {
            counters.Skip.Increment();
            logger.LogError(
                "Multiple classifications ({Classifications}) found for file {File}.",
                string.Join(", ", index.Classifications.Select(x => x.Source)),
                index.Filepath
            );
        }

        if (index.Classifications.Count() == 1)
        {
            counters.Done.Increment();
            counters.Out.Increment();
            ReportResult?.Invoke(this, new(index));
        }
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
