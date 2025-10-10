using TheFipster.ActivityAggregator.Api.Services.Contracts;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Api.Services.Decoration;

public class UnzipIndexer(IUnzipService component, IIndexer<ZipIndex> indexer) : IUnzipService
{
    public async Task<ZipIndex> ExtractAsync(
        string zipFilepath,
        string outputDirectory,
        CancellationToken ct
    )
    {
        var index = await component.ExtractAsync(zipFilepath, outputDirectory, ct);

        indexer.Set(index);

        return index;
    }
}
