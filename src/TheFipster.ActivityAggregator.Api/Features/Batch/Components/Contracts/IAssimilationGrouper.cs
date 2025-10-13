using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Files;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Api.Features.Batch.Components.Contracts;

public interface IAssimilationGrouper
{
    [Obsolete]
    Task<BatchIndex> CombinePerDayAsync(InventoryIndex item, DataKind kind, CancellationToken ct);
    Task<Dictionary<MergedFile, List<AssimilateIndex>>> CombinePerDayAsync(
        DateTime day,
        CancellationToken ct
    );
}
