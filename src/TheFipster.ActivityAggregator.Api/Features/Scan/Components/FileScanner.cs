using TheFipster.ActivityAggregator.Api.Features.Scan.Components.Contracts;
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

        var index = fileInventory.GetById(hash);
        if (index != null)
        {
            if (!index.AlternateFiles.Contains(filepath))
                index.AlternateFiles.Add(filepath);

            if (index.Source.HasValue)
                return index;
        }

        index = new FileIndex
        {
            Hash = hash,
            ZipHash = zipHash,
            Size = file.Length,
            Path = filepath,
        };

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
        index.Source = result.Classification?.Source;
        index.Timestamp = result.Classification?.Datetime;
        index.Range = result.Classification?.Range;

        return index;
    }
}
