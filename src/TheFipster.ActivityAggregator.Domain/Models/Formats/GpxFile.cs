using System.Globalization;
using System.Xml.Linq;
using TheFipster.ActivityAggregator.Domain.Models.Components;

namespace TheFipster.ActivityAggregator.Domain.Models.Formats
{
    public class GpxFile
    {
        private readonly XDocument _doc;
        private readonly XNamespace _xmlns;

        private readonly string[] _supportedNs =
        {
            "http://www.topografix.com/GPX/1/0",
            "http://www.topografix.com/GPX/1/1",
        };

        public GpxFile(string filepath)
        {
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
            var points = _doc.Descendants(_xmlns + "trkpt")
                .Select(pt => new GpxPoint(
                    double.Parse(pt.Attribute("lat")?.Value, CultureInfo.InvariantCulture),
                    double.Parse(pt.Attribute("lon").Value, CultureInfo.InvariantCulture),
                    pt.Element(_xmlns + "ele") != null
                        ? double.Parse(
                            pt.Element(_xmlns + "ele").Value,
                            CultureInfo.InvariantCulture
                        )
                        : null,
                    pt.Element(_xmlns + "time") != null
                        ? DateTime.Parse(
                            pt.Element(_xmlns + "time").Value,
                            null,
                            DateTimeStyles.AdjustToUniversal
                        )
                        : null,
                    pt.Element(_xmlns + "speed") != null
                        ? double.Parse(
                            pt.Element(_xmlns + "speed").Value,
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
