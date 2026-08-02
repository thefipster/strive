namespace TheFipster.ActivityAggregator.Domain.Models.Importing;

public class NormalizedResult
{
    public UnifiedSamples? Samples { get; set; }
    public UnifiedTrack? Track { get; set; }
    public List<double>? Pulses { get; set; }
}
