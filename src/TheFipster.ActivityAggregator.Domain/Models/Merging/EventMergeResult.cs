using TheFipster.ActivityAggregator.Domain.Enums;

namespace TheFipster.ActivityAggregator.Domain.Models;

public record EventMergeResult(List<UnifiedEvent> Resolved);
