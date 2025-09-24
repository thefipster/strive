using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Domain.Configs;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Extensions;
using TheFipster.ActivityAggregator.Domain.Models.Extraction;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Scanner;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Importer.Abstractions;
using TheFipster.ActivityAggregator.Importer.Modules.Abstractions;
using TheFipster.ActivityAggregator.Services.Abstractions;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Services;

public class Scanner(
    IOptions<ScannerConfig> config,
    IImporterRegistry registry,
    IIndexer<ScannerIndex> indexer,
    ILogger<Scanner> logger
) : IScanner
{
    public async Task<ScannerIndex> CheckAsync(string filepath, CancellationToken ct)
    {
        var file = new FileInfo(filepath);
        var hash = await file.HashXx3Async(ct);
        var index = indexer.GetById(hash);

        if (IndexIsAlreadyPublished(index))
            return index!;

        index = EnsureIndexExists(index, hash);

        AppendFileWhenNew(index, file);

        if (ExcludedByFileExtension(file, index))
            return index;

        var classifications = Classify(file);

        Validate(classifications, index);

        return index;
    }

    private bool IndexIsAlreadyPublished(ScannerIndex? index)
    {
        if (index?.Classification == null)
            return false;

        var classifier = LoadClassifier(index.Classification);
        if (NewerClassifierAvailable(index, classifier))
            return false;

        if (LastMessageIsNotNoOperation(index))
            index.Actions.Add(DateTime.UtcNow, ScannerActions.NoOperation);

        return true;
    }

    private IFileClassifier LoadClassifier(Classification classification)
    {
        var classifier = registry
            .LoadClassifiers()
            .FirstOrDefault(x => x.Source == classification.Source);

        return classifier
            ?? throw new ClassificationException(
                $"Couldn't load classifier {classification.Source}"
            );
    }

    private ScannerIndex EnsureIndexExists(ScannerIndex? index, string hash)
    {
        if (index == null)
            index = new ScannerIndex { Hash = hash };

        return index;
    }

    private static void AppendFileWhenNew(ScannerIndex index, FileInfo file)
    {
        if (!index.Files.Contains(file.FullName))
            index.Files.Add(file.FullName);
    }

    private bool ExcludedByFileExtension(FileInfo file, ScannerIndex index)
    {
        if (!config.Value.ExcludedFileExtensions.Contains(file.Extension))
            return false;

        index.IsExcluded = true;
        index.ExclusionReason = ScannerRejections.FileExtensionExcluded;
        index.Actions.Log(ScannerActions.ExcludedByFileExtension);
        return true;
    }

    private List<ClassificationResult> Classify(FileInfo file)
    {
        var probe = new FileProbe(file.FullName);
        var classifications = new List<ClassificationResult>();
        foreach (var classifier in registry.LoadClassifiers())
        {
            var classification = TryClassification(file.FullName, classifier, probe);
            classification.Version = classifier.ClassifierVersion;
            classifications.Add(classification);
        }

        return classifications;
    }

    private static void Validate(List<ClassificationResult> classifications, ScannerIndex index)
    {
        var positives = classifications
            .Where(result => result is { Classification: not null, Exception: null })
            .ToList();

        if (positives.Count == 0)
        {
            index.Actions.Log(ScannerActions.NotClassified);
        }

        if (positives.Count == 1)
        {
            index.Classification = positives.First().Classification;
            index.Actions.Log(ScannerActions.Classified);
        }

        if (positives.Count > 1)
        {
            index.Conflicts = positives.Select(x => x.Classification).ToList();
            index.Actions.Log(ScannerActions.Conflict);
        }
    }

    private ClassificationResult TryClassification(
        string filepath,
        IFileClassifier importer,
        FileProbe probe
    )
    {
        var result = new ClassificationResult(importer.Source);

        try
        {
            var import = importer.Classify(probe);
            result.Classification = import.ToClassification();
            result.Version = importer.ClassifierVersion;
        }
        catch (ClassificationException e)
        {
            result.Exception = e;
        }
        catch (Exception e)
        {
            result.Exception = e;
            WriteLog(filepath, importer, e);
        }

        return result;
    }

    private void WriteLog(string filepath, IFileClassifier classifier, Exception e)
    {
        logger.LogError(
            e,
            "Unexpected error while classifying file {ImportFile} using classifier {Classifier}.",
            filepath,
            classifier.Source
        );
    }

    private bool NewerClassifierAvailable(ScannerIndex index, IFileClassifier classifier) =>
        index.Classification != null && classifier.ClassifierVersion > index.Classification.Version;

    private bool LastMessageIsNotNoOperation(ScannerIndex index) =>
        index.Actions.Count != 0
        && index.Actions.OrderByDescending(x => x.Key).First().Value != ScannerActions.NoOperation;
}
