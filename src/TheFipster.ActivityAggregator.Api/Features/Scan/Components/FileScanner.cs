using TheFipster.ActivityAggregator.Domain.Extensions;
using TheFipster.ActivityAggregator.Domain.Models.Importing;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Api.Features.Scan.Components;

public class FileScanner(IClassifier classifier, IIndexer<FileIndex> fileInventory) : IFileScanner
{
    public async Task<FileIndex> HandleFile(string filepath, string zipHash, CancellationToken ct)
    {
        var file = new FileInfo(filepath);
        var hash = await file.HashXx3Async(ct);

        if (FileIsAlreadyIndexed(filepath, hash, out var index))
            return index!;

        index = FileIndex.New(hash, zipHash, file.Length, filepath);
        List<ClassificationResult> results;
        try
        {
            results = classifier.Classify(file, ct);
        }
        catch (Exception)
        {
            return index;
        }

        if (results.Count(x => x.Classification != null) != 1)
            return index;

        var result = results.First(x => x.Classification != null);
        index.SetClassification(result.Classification);

        return index;
    }

    private bool FileIsAlreadyIndexed(string filepath, string hash, out FileIndex? index)
    {
        index = fileInventory.GetById(hash);
        if (index == null)
            return false;

        if (!index.AlternateFiles.Contains(filepath))
            index.AlternateFiles.Add(filepath);

        if (index.Source.HasValue)
            return true;

        return false;
    }
}
