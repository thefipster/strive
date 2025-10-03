using TheFipster.ActivityAggregator.Domain.Models.Unified;

namespace TheFipster.ActivityAggregator.Domain.Models.Merging;

public class NormalizedResult
{
    public UnifiedSamples? Samples { get; set; }
    public UnifiedTrack? Track { get; set; }
    public List<double>? Pulses { get; set; }
}
