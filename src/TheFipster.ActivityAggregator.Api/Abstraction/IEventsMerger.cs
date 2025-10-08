using TheFipster.ActivityAggregator.Domain.Models.Importing;

namespace TheFipster.ActivityAggregator.Api.Abstraction;

public interface IEventsMerger
{
    EventMergeResult Merge(params UnifiedEvent[][] events);
}
