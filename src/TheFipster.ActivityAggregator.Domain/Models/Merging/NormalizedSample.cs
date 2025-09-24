namespace TheFipster.ActivityAggregator.Domain.Models.Merging;

public class NormalizedSample
{
    public DateTime Timestamp { get; set; }
    public Dictionary<Parameters, double> Values { get; set; } = new();
}
