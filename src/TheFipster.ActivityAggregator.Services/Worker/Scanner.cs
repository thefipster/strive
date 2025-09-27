using System.Data.Common;
using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Domain.Configs;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Extensions;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Scanner;
using TheFipster.ActivityAggregator.Importer.Abstractions;
using TheFipster.ActivityAggregator.Services.Abstractions;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Services.Worker;

public class Scanner(
    IOptions<ScannerConfig> config,
    IIndexer<ScannerIndex> indexer,
    IClassifier classifier
) : IScanner
{
    public async Task<ScannerIndex> CheckAsync(string filepath, CancellationToken ct)
    {
        var file = new FileInfo(filepath);
        var hash = await file.HashXx3Async(ct);
        var index = indexer.GetById(hash);

        if (IndexIsAlreadyPublished(index, file))
            return Updated(index!);

        index = EnsureIndexExists(index, hash);
        AppendFileWhenNew(index, file);
        if (ExcludedByFileExtension(file, index))
            return Updated(index);

        var classifications = classifier.Classify(file, ct);
        index.IndexedAt = DateTime.UtcNow;
        Validate(classifications, index);

        return Updated(index);
    }

    private bool IndexIsAlreadyPublished(ScannerIndex? index, FileInfo file)
    {
        if (index?.Classification == null)
            return false;

        var item = classifier.GetClassifier(index.Classification.Source);
        if (NewerClassifierAvailable(index, item))
            return false;

        if (LastMessageIsNotNoOperation(index))
            index.Actions.Add(DateTime.UtcNow, ScannerActions.NoOperation);

        AppendFileWhenNew(index, file);

        return true;
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

    private static void Validate(List<ClassificationResult> classifications, ScannerIndex index)
    {
        var positives = classifications
            .Where(result => result is { Classification: not null, Exception: null })
            .ToList();

        if (positives.Count == 0)
        {
            index.Actions.Log(ScannerActions.NotClassified);
            return;
        }

        if (positives.Count == 1)
        {
            index.Classification = positives.First().Classification;
            index.Actions.Log(ScannerActions.Classified);
            return;
        }

        index.Conflicts = positives.Select(x => x.Classification).ToList();
        index.Actions.Log(ScannerActions.Conflict);
    }

    private ScannerIndex Updated(ScannerIndex index)
    {
        indexer.Set(index);
        return index;
    }

    private bool NewerClassifierAvailable(ScannerIndex index, IFileClassifier specificClassifier) =>
        index.Classification != null
        && specificClassifier.ClassifierVersion > index.Classification.Version;

    private bool LastMessageIsNotNoOperation(ScannerIndex index) =>
        index.Actions.Count != 0
        && index.Actions.OrderByDescending(x => x.Key).First().Value != ScannerActions.NoOperation;
}
