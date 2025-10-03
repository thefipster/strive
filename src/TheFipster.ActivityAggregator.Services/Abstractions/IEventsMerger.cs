using TheFipster.ActivityAggregator.Domain.Models;

namespace TheFipster.ActivityAggregator.Services.Abstractions;

public interface IEventsMerger
{
    EventMergeResult Merge(params UnifiedEvent[][] events);
}
