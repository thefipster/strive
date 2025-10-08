using System.Globalization;
using System.Xml.Linq;

namespace TheFipster.ActivityAggregator.Domain.Models.Formats
{
    public class GpxFile
    {
        private bool _skipOnError;
        private readonly XDocument _doc;
        private readonly XNamespace _xmlns;

        private readonly string[] _supportedNs =
        {
            "http://www.topografix.com/GPX/1/0",
            "http://www.topografix.com/GPX/1/1",
        };

        public GpxFile(string filepath, bool skipOnError = false)
        {
            _skipOnError = skipOnError;

            if (!File.Exists(filepath))
                throw new ArgumentException($"File {filepath} doesn't exist.");

            var text = File.ReadAllText(filepath);
            _doc = XDocument.Parse(text);

            if (_doc.Root == null || _doc.Root.Name.LocalName != "gpx")
                throw new ArgumentException("File is not gpx");

            _xmlns = _doc.Root.Name.Namespace;
            if (!_supportedNs.Contains(_xmlns.NamespaceName))
                throw new NotSupportedException($"File version not supported: {_xmlns}");
        }

        public IEnumerable<GpxPoint> GetPoints()
        {
            var coordinates = new List<GpxPoint>();

            foreach (var item in _doc.Descendants(_xmlns + "trkpt"))
            {
                try
                {
                    var coordinate = CreatePoint(item);
                    coordinates.Add(coordinate);
                }
                catch (Exception)
                {
                    if (!_skipOnError)
                        throw;
                }
            }

            return coordinates;
        }

        private GpxPoint CreatePoint(XElement trackPoint)
        {
            var latitude = trackPoint.Attribute("lat")?.Value;
            var longitude = trackPoint.Attribute("lon")?.Value;

            if (string.IsNullOrWhiteSpace(latitude) || string.IsNullOrWhiteSpace(longitude))
                throw new ArgumentException("Trackpoint has no lat/lon pair.");

            var coordinate = new GpxPoint
            {
                Latitude = double.Parse(latitude, CultureInfo.InvariantCulture),
                Longitude = double.Parse(longitude, CultureInfo.InvariantCulture),
            };

            var ele = trackPoint.Element(_xmlns + "ele")?.Value;
            if (!string.IsNullOrWhiteSpace(ele))
                coordinate.Altitude = double.Parse(ele, CultureInfo.InvariantCulture);

            var time = trackPoint.Element(_xmlns + "time")?.Value;
            if (!string.IsNullOrWhiteSpace(time))
                coordinate.Timestamp = DateTime.Parse(time);

            var speed = trackPoint.Element(_xmlns + "speed")?.Value;
            if (!string.IsNullOrWhiteSpace(speed))
                coordinate.Speed = double.Parse(speed, CultureInfo.InvariantCulture);

            return coordinate;
        }
    }
}
