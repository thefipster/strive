using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Files;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Api.Features.Batch.Components.Contracts;

public interface IPessimisticMerger
{
    MergedFile CombineAssimilationGroup(
        DateTime timestamp,
        DataKind kind,
        List<AssimilateIndex> assimilations,
        CancellationToken ct
    );
}
