namespace TheFipster.ActivityAggregator.Domain.Models.Formats;

public class GpxPoint : GpsPoint
{
    public DateTime? Timestamp { get; set; }
    public double? Speed { get; set; }
}
