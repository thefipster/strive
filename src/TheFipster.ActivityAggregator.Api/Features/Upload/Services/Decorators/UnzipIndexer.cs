using TheFipster.ActivityAggregator.Api.Features.Upload.Services.Contracts;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Api.Features.Upload.Services.Decorators;

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
