namespace TheFipster.ActivityAggregator.Domain.Models.Formats;

public class GpxPoint : GpsPoint
{
    public GpxPoint() { }

    public GpxPoint(double lat, double lon, double? elevation, DateTime? timestamp, double? speed)
        : base(lat, lon)
    {
        if (elevation.HasValue)
            Altitude = elevation.Value;

        if (speed.HasValue)
            Speed = speed.Value;

        if (timestamp.HasValue)
            Timestamp = timestamp.Value;
    }

    public DateTime? Timestamp { get; set; }
    public double? Speed { get; set; }
}
