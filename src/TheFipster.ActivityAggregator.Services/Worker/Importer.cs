using HashifyNet.Algorithms.RapidHash;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Extensions;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Services.Abstractions;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Services.Worker;

public class Importer(IUnzipper unzip, IIndexer<ImporterIndex> indexer) : IImporter
{
    public async Task<ImporterIndex> ReadAsync(
        string zipFilepath,
        string outputDirectory,
        CancellationToken ct
    )
    {
        var file = new FileInfo(zipFilepath);
        var hash = await file.HashXx3Async(ct);
        var outputName = file.Name.Replace(file.Extension, string.Empty);
        var outputPath = Path.Combine(outputDirectory, outputName);

        var index = indexer.GetById(hash);
        if (IndexIsAlreadyPublished(index, file))
            return Updated(index!);

        unzip.Extract(zipFilepath, outputPath, true);

        index = new ImporterIndex { Hash = hash, Output = outputPath };
        index.Files.Add(file.FullName);
        index.Actions.Log(ImporterActions.Extracted);
        return Updated(index);
    }

    private bool IndexIsAlreadyPublished(ImporterIndex? index, FileInfo file)
    {
        if (index == null)
            return false;

        if (!index.Files.Contains(file.FullName))
            index.Files.Add(file.FullName);

        index.Actions.Log(ImporterActions.NoOperation);
        return true;
    }

    private ImporterIndex Updated(ImporterIndex index)
    {
        indexer.Set(index);
        return index;
    }
}
