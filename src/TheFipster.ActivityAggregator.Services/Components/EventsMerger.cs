using TheFipster.ActivityAggregator.Domain.Models.Merging;
using TheFipster.ActivityAggregator.Domain.Models.Unified;
using TheFipster.ActivityAggregator.Services.Abstractions;

namespace TheFipster.ActivityAggregator.Services.Components;

public class EventsMerger : IEventsMerger
{
    public EventMergeResult Merge(params UnifiedEvent[][] events)
    {
        var all = events.SelectMany(x => x).ToList();
        return new EventMergeResult(all);
    }
}
