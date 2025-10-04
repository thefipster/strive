using TheFipster.ActivityAggregator.Domain.Models.Merging;
using TheFipster.ActivityAggregator.Domain.Models.Unified;

namespace TheFipster.ActivityAggregator.Api.Abstraction;

public interface IEventsMerger
{
    EventMergeResult Merge(params UnifiedEvent[][] events);
}
