namespace TheFipster.ActivityAggregator.Domain.Models.Importing;

public class WeightEntry
{
    public double Weight { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.UtcNow;
}
