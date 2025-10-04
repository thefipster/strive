using TheFipster.ActivityAggregator.Api.Abstraction;
using TheFipster.ActivityAggregator.Domain.Models.Merging;
using TheFipster.ActivityAggregator.Domain.Models.Unified;

namespace TheFipster.ActivityAggregator.Api.Components;

public class EventsMerger : IEventsMerger
{
    public EventMergeResult Merge(params UnifiedEvent[][] events)
    {
        var all = events.SelectMany(x => x).ToList();
        return new EventMergeResult(all);
    }
}
