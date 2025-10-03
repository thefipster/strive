using System.Globalization;
using System.Xml.Linq;
using TheFipster.ActivityAggregator.Domain.Models.Components;

namespace TheFipster.ActivityAggregator.Domain.Models.Formats
{
    public class KmlFile
    {
        private readonly XDocument doc;

        public KmlFile(string filepath)
        {
            if (!File.Exists(filepath))
                throw new ArgumentException($"File {filepath} doesn't exist.");

            var text = File.ReadAllText(filepath);
            doc = XDocument.Parse(text);

            if (doc.Root == null || doc.Root.Name.LocalName != "kml")
                throw new ArgumentException("File is not kml");
        }

        public IEnumerable<GpsPoint> GetPoints()
        {
            XNamespace ns1 = "http://earth.google.com/kml/2.1";
            XNamespace ns2 = "http://www.opengis.net/kml/2.2";

            var linesString = doc.Descendants(ns1 + "LineString")
                .Elements(ns1 + "coordinates")
                .ToArray();

            if (linesString.Length == 0)
            {
                linesString = doc.Descendants(ns2 + "LineString")
                    .Elements(ns2 + "coordinates")
                    .ToArray();
            }

            var coordinates = linesString.FirstOrDefault()?.Value;
            if (coordinates != null)
            {
                return coordinates
                    .Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(coord =>
                    {
                        var parts = coord.Split(',');
                        var lat = double.Parse(parts[1], CultureInfo.InvariantCulture);
                        var lon = double.Parse(parts[0], CultureInfo.InvariantCulture);
                        double? alt =
                            parts.Count() > 2
                                ? double.Parse(parts[2], CultureInfo.InvariantCulture)
                                : null;
                        return new GpsPoint(lat, lon, alt);
                    })
                    .ToList();
            }

            throw new Exception("Couldn't read kml coordinates.");
        }
    }
}
