using TheFipster.ActivityAggregator.Domain.Models.Importing;

namespace TheFipster.ActivityAggregator.Api.Features.Batch.Components.Contracts;

public interface IEventsMerger
{
    EventMergeResult Merge(params UnifiedEvent[][] events);
}
