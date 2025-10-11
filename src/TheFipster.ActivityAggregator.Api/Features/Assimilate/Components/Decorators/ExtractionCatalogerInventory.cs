using TheFipster.ActivityAggregator.Domain.Models.Files;
using TheFipster.ActivityAggregator.Domain.Models.Importing;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Api.Features.Assimilate.Components.Decorators;

public class ExtractionCatalogerInventory(
    IExtractionCataloger component,
    IInventoryIndexer inventory
) : IExtractionCataloger
{
    public async Task<ExtractionMeta> HandleExtractAsync(
        FileIndex file,
        FileExtraction extract,
        CancellationToken ct
    )
    {
        var meta = await component.HandleExtractAsync(file, extract, ct);

        var entry = new InventoryIndex(meta.Timestamp, meta.Range);
        inventory.EnsureIndex(entry);

        return meta;
    }
}
