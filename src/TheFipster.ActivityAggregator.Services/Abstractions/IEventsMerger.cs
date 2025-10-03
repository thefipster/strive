using TheFipster.ActivityAggregator.Domain.Models.Merging;
using TheFipster.ActivityAggregator.Domain.Models.Unified;

namespace TheFipster.ActivityAggregator.Services.Abstractions;

public interface IEventsMerger
{
    EventMergeResult Merge(params UnifiedEvent[][] events);
}
