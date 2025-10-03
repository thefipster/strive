namespace TheFipster.ActivityAggregator.Domain.Models.Components
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

        public GpsPoint(string lat, string lon, string? alt)
        {
            Latitude = double.Parse(lat);
            Longitude = double.Parse(lon);

            if (alt != null)
                Altitude = double.Parse(alt);
        }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Altitude { get; set; }
    }
}
