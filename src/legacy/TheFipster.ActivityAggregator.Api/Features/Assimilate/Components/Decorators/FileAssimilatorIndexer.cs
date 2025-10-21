using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Abstractions.Features.Indexing.Components;

namespace TheFipster.ActivityAggregator.Api.Features.Assimilate.Components.Decorators;

public class FileAssimilatorIndexer(IFileAssimilator component, IIndexer<ExtractorIndex> indexer)
    : IFileAssimilator
{
    public async Task<ExtractorIndex?> ConvergeFileAsync(FileIndex file, CancellationToken ct)
    {
        var index = await component.ConvergeFileAsync(file, ct);

        if (index != null)
            indexer.Set(index);

        return index;
    }
}
