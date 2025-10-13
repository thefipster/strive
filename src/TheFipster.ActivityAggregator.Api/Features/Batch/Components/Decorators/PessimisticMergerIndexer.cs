using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Files;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Api.Features.Batch.Components.Decorators;

public class PessimisticMergerIndexer(IPessimisticMerger component, IIndexer<BatchIndex> indexer)
    : IPessimisticMerger
{
    public async Task<BatchIndex> HandleAssimilationGroupAsync(
        DateTime timestamp,
        DataKind kind,
        List<AssimilateIndex> assimilations,
        CancellationToken ct
    )
    {
        var index = await component.HandleAssimilationGroupAsync(
            timestamp,
            kind,
            assimilations,
            ct
        );
        indexer.Set(index);
        return index;
    }

    public MergedFile CombineAssimilationGroup(
        DateTime timestamp,
        DataKind kind,
        List<AssimilateIndex> assimilations,
        CancellationToken ct
    )
    {
        return component.CombineAssimilationGroup(timestamp, kind, assimilations, ct);
    }
}
