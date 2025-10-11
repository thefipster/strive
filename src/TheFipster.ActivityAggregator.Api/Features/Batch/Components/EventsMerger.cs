using TheFipster.ActivityAggregator.Api.Features.Batch.Components.Contracts;
using TheFipster.ActivityAggregator.Domain.Models.Importing;

namespace TheFipster.ActivityAggregator.Api.Features.Batch.Components;

public class EventsMerger : IEventsMerger
{
    public EventMergeResult Merge(params UnifiedEvent[][] events)
    {
        var all = events.SelectMany(x => x).ToList();
        return new EventMergeResult(all);
    }
}
