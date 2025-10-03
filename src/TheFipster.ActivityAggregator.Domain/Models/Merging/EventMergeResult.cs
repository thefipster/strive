using TheFipster.ActivityAggregator.Domain.Models.Unified;

namespace TheFipster.ActivityAggregator.Domain.Models.Merging;

public record EventMergeResult(List<UnifiedEvent> Resolved);
