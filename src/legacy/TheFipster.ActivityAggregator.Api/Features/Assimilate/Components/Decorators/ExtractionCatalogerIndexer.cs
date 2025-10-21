using TheFipster.ActivityAggregator.Domain.Models.Files;
using TheFipster.ActivityAggregator.Domain.Models.Importing;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Abstractions.Features.Indexing.Components;

namespace TheFipster.ActivityAggregator.Api.Features.Assimilate.Components.Decorators;

public class ExtractionCatalogerIndexer(
    IExtractionCataloger component,
    IIndexer<AssimilateIndex> indexer
) : IExtractionCataloger
{
    public async Task<ExtractionMeta> HandleExtractAsync(
        FileIndex file,
        FileExtraction extract,
        CancellationToken ct
    )
    {
        var meta = await component.HandleExtractAsync(file, extract, ct);

        AssimilateIndex index = AssimilateIndex.New(meta, file);
        indexer.Set(index);

        return meta;
    }
}
