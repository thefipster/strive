namespace TheFipster.ActivityAggregator.Domain
{
    public class GpsPoint
    {
        public GpsPoint() { }

        public GpsPoint(double lat, double lon)
        {
            Latitude = lat;
            Longitude = lon;
        }

        public GpsPoint(double lat, double lon, double? alt)
            : this(lat, lon)
        {
            Altitude = alt;
        }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Altitude { get; set; }
    }
}
