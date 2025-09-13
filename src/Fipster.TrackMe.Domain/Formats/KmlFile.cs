using System.Globalization;
using System.Xml.Linq;

namespace Fipster.TrackMe.Domain.Formats
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

        public static void Write(string filepath, GenericCollectionFile<GpsPoint> track)
        {
            var info = new FileInfo(filepath);
            XNamespace ns = "http://www.opengis.net/kml/2.2";

            // Build coordinates string "lon,lat,alt"
            string coordinates = string.Join(
                " ",
                track
                    .Data.ToList()
                    .ConvertAll(p =>
                        $"{p.Longitude.ToString(CultureInfo.InvariantCulture)},{p.Latitude.ToString(CultureInfo.InvariantCulture)},0"
                    )
            );

            new XDocument(
                new XElement(
                    ns + "kml",
                    new XElement(
                        ns + "Document",
                        new XElement(
                            ns + "Placemark",
                            new XElement(ns + "name", info.Name),
                            new XElement(
                                ns + "Style",
                                new XAttribute("id", "orangeLine"),
                                new XElement(
                                    ns + "LineStyle",
                                    new XElement(ns + "color", "ff00a5ff"),
                                    new XElement(ns + "width", "3")
                                )
                            ),
                            new XElement(
                                ns + "LineString",
                                new XElement(ns + "coordinates", coordinates)
                            )
                        )
                    )
                )
            ).Save(filepath);
            ;
        }

        public IEnumerable<GpsPoint> GetPoints()
        {
            XNamespace ns1 = "http://earth.google.com/kml/2.1";
            XNamespace ns2 = "http://www.opengis.net/kml/2.2";

            var coordinatess = doc.Descendants(ns1 + "LineString").Elements(ns1 + "coordinates");

            if (coordinatess.Count() == 0)
            {
                coordinatess = doc.Descendants(ns2 + "LineString").Elements(ns2 + "coordinates");
            }

            var coordinates = coordinatess.FirstOrDefault()?.Value;

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
