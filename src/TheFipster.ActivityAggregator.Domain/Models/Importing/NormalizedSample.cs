using TheFipster.ActivityAggregator.Domain.Enums;

namespace TheFipster.ActivityAggregator.Domain.Models.Importing;

public class NormalizedSample
{
    public DateTime Timestamp { get; set; }
    public Dictionary<Parameters, double> Values { get; set; } = new();
}
