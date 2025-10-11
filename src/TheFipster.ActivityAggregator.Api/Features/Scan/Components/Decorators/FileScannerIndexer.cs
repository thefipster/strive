using TheFipster.ActivityAggregator.Api.Features.Scan.Components.Contracts;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Api.Features.Scan.Components.Decorators;

public class FileScannerIndexer(IFileScanner component, IIndexer<FileIndex> indexer) : IFileScanner
{
    public async Task<FileIndex> HandleFile(string filepath, string zipHash, CancellationToken ct)
    {
        var index = await component.HandleFile(filepath, zipHash, ct);

        indexer.Set(index);

        return index;
    }
}
