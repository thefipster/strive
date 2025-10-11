using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Api.Features.Batch.Components;

public class AssimilationGrouper(IPessimisticMerger merger, IIndexer<AssimilateIndex> indexer)
    : IAssimilationGrouper
{
    public async Task<BatchIndex> CombinePerDay(
        InventoryIndex item,
        DataKind kind,
        CancellationToken ct
    )
    {
        var assimilations = indexer
            .GetFiltered(x => x.Kind == kind && x.Timestamp == item.Timestamp)
            .ToList();

        return await merger.HandleAssimilationGroupAsync(item.Timestamp, kind, assimilations, ct);
    }
}
