using TheFipster.ActivityAggregator.Domain.Enums;

namespace TheFipster.ActivityAggregator.Domain.Models;

public class UnifiedSamples
{
    public SampleTypes Type { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public List<DateTime> Timeline { get; set; } = [];
    public Dictionary<Parameters, IEnumerable<double>> Values { get; set; } = new();
}
