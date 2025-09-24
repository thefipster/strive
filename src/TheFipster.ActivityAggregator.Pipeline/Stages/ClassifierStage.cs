using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Scanner;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Importer.Abstractions;
using TheFipster.ActivityAggregator.Importer.Modules.Abstractions;
using TheFipster.ActivityAggregator.Pipeline.Abstractions;
using TheFipster.ActivityAggregator.Pipeline.Config;
using TheFipster.ActivityAggregator.Pipeline.Models;
using TheFipster.ActivityAggregator.Pipeline.Models.Events;
using TheFipster.ActivityAggregator.Pipeline.Pipelines;
using TheFipster.ActivityAggregator.Storage.Abstractions;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Pipeline.Stages;

public class ClassifierStage(
    PipelineState<IngesterPipeline> state,
    IOptions<ClassifierConfig> config,
    IImporterRegistry registry,
    IIndexer<ClassificationIndex> indexer,
    ILogger<ClassifierStage> logger
) : Stage<ScanIndex, ClassificationIndex>, IClassifierStage
{
    public int Version => 1;
    public int Order => 20;
    public event EventHandler<ResultReportEventArgs<ClassificationIndex>>? ReportResult;

    private readonly IEnumerable<IFileClassifier> classifiers = registry.LoadClassifiers();

    public async Task ExecuteAsync(CancellationToken ct)
    {
        var tasks = new List<Task>();
        for (int i = 0; i < config.Value.MaxDegreeOfParallelismDequeing; i++)
            tasks.Add(CreateClassifyTask(ct));

        await Task.WhenAll(tasks);
        state.FinishedStages.Add(GetType().Name);
    }

    private Task CreateClassifyTask(CancellationToken ct) =>
        Task.Run(async () => await LoopAsync(ct), ct);

    private async Task LoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested && JobsAvailable)
            await TryDequeue(ct);
    }

    private bool JobsAvailable =>
        !state.FinishedStages.Contains(nameof(ScannerStage)) || !queue.IsEmpty;

    private async Task TryDequeue(CancellationToken ct)
    {
        if (queue.TryDequeue(out var input))
        {
            try
            {
                ProcessInput(input);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Bundling failed for input {Input}.", input);
            }

            Counters.Done.Increment();
        }
        else
        {
            await Task.Delay(10, ct);
        }
    }

    private void ProcessInput(ScanIndex scan)
    {
        var index = indexer.GetById(scan.Filepath);
        var file = new FileInfo(scan.Filepath);
        if (index == null || index.IndexedAt < file.LastWriteTimeUtc || index.Version < Version)
        {
            var classifications = new ConcurrentBag<ImportClassification>();
            Parallel.ForEach(
                classifiers,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = config.Value.MaxDegreeOfParallelismClassifying,
                },
                classifier => TryClassify(scan, classifier, classifications)
            );

            index = new ClassificationIndex(
                Version,
                scan.Filepath,
                classifications.Select(result => result.ToClassification())
            );

            indexer.Set(index);
            ReportClassificationResult(index);
        }
    }

    private void TryClassify(
        ScanIndex scan,
        IFileClassifier classifier,
        ConcurrentBag<ImportClassification> classifications
    )
    {
        try
        {
            var probe = new FileProbe(scan.Filepath);
            var result = classifier.Classify(probe);

            if (result.Datetype != DateRanges.AllTime && result.Datetime == DateTime.MinValue)
            {
                var directory = new FileInfo(probe.Filepath).Directory;
                var date = DateHelper.GetDateFromMyCollectionDirectory(directory);
                result.Datetime = date;
            }

            classifications.Add(result);
        }
        catch (ClassificationException e)
        {
            logger.LogTrace(e, "Classification miss.");
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Classification failed for file {File} using {Classifier}.",
                scan.Filepath,
                classifier.Source
            );
        }
    }

    private void ReportClassificationResult(ClassificationIndex index)
    {
        if (!index.Classifications.Any())
        {
            Counters.Skip.Increment();
            logger.LogError("No classification found for file {File}.", index.Filepath);
        }

        if (index.Classifications.Count() > 1)
        {
            Counters.Skip.Increment();
            logger.LogError(
                "Multiple classifications ({Classifications}) found for file {File}.",
                string.Join(", ", index.Classifications.Select(x => x.Source)),
                index.Filepath
            );
        }

        if (index.Classifications.Count() == 1)
        {
            Counters.Out.Increment();
            ReportResult?.Invoke(this, new(index));
        }
    }
}
