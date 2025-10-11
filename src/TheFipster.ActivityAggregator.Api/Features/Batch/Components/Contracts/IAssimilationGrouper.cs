using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Api.Features.Batch.Components.Contracts;

public interface IAssimilationGrouper
{
    Task<BatchIndex> CombinePerDay(InventoryIndex item, DataKind kind, CancellationToken ct);
}
