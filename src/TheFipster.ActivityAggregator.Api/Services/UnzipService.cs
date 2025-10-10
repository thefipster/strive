using TheFipster.ActivityAggregator.Api.Components.Contracts;
using TheFipster.ActivityAggregator.Api.Services.Contracts;
using TheFipster.ActivityAggregator.Domain.Extensions;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Api.Services;

public class UnzipService(IUnzipper unzip, IIndexer<ZipIndex> indexer) : IUnzipService
{
    public async Task<ZipIndex> ExtractAsync(
        string zipFilepath,
        string outputDirectory,
        CancellationToken ct
    )
    {
        var file = new FileInfo(zipFilepath);
        var hash = await file.HashXx3Async(ct);

        if (ZipFileIsAlreadyIndexed(zipFilepath, hash, out var index))
            return index!;

        var stats = unzip.Extract(zipFilepath, true);
        var zipSize = file.Length;

        return ZipIndex.New(hash, zipFilepath, zipSize, stats);
    }

    private bool ZipFileIsAlreadyIndexed(string zipFilepath, string hash, out ZipIndex? index)
    {
        index = indexer.GetById(hash);

        if (index == null)
            return false;

        if (zipFilepath != index.ZipPath)
            index.AlternateFiles.Add(zipFilepath);

        return true;
    }
}
