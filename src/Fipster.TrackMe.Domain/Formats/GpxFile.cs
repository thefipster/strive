using System.Globalization;
using System.Xml.Linq;
using Fipster.TrackMe.Domain.Models;

namespace Fipster.TrackMe.Domain.Formats
{
    public class GpxFile
    {
        private readonly XDocument doc;
        private readonly XNamespace xmlns;

        private readonly string[] supportedNs =
        {
            "http://www.topografix.com/GPX/1/0",
            "http://www.topografix.com/GPX/1/1",
        };

        public GpxFile(string filepath)
        {
            if (!File.Exists(filepath))
                throw new ArgumentException($"File {filepath} doesn't exist.");

            var text = File.ReadAllText(filepath);
            doc = XDocument.Parse(text);

            if (doc.Root == null || doc.Root.Name.LocalName != "gpx")
                throw new ArgumentException("File is not gpx");

            xmlns = doc.Root.Name.Namespace;
            if (!supportedNs.Contains(xmlns.NamespaceName))
                throw new NotSupportedException($"File version not supported: {xmlns}");
        }

        public IEnumerable<GpxPoint> GetPoints()
        {
            var points = doc.Descendants(xmlns + "trkpt")
                .Select(pt => new GpxPoint(
                    double.Parse(pt.Attribute("lat").Value, CultureInfo.InvariantCulture),
                    double.Parse(pt.Attribute("lon").Value, CultureInfo.InvariantCulture),
                    pt.Element(xmlns + "ele") != null
                        ? double.Parse(
                            pt.Element(xmlns + "ele").Value,
                            CultureInfo.InvariantCulture
                        )
                        : null,
                    pt.Element(xmlns + "time") != null
                        ? DateTime.Parse(
                            pt.Element(xmlns + "time").Value,
                            null,
                            DateTimeStyles.AdjustToUniversal
                        )
                        : null,
                    pt.Element(xmlns + "speed") != null
                        ? double.Parse(
                            pt.Element(xmlns + "speed").Value,
                            CultureInfo.InvariantCulture
                        )
                        : null
                ));

            if (points != null && points.Count() > 0)
                return points;

            throw new Exception("Couldn't read gpx coordinates.");
        }
    }
}
