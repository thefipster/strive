using TheFipster.ActivityAggregator.Domain.Models.Importing;

namespace TheFipster.ActivityAggregator.Api.Components.Contracts;

public interface IEventsMerger
{
    EventMergeResult Merge(params UnifiedEvent[][] events);
}
