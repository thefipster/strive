using System.Globalization;
using Fipster.TrackMe.Domain;
using Fipster.TrackMe.Domain.Formats;
using Fipster.TrackMe.Domain.Models;
using Fipster.TrackMe.Domain.Tools;
using Fipster.TrackMe.Importer.Modules.Abstractions;

namespace Fipster.TrackMe.Importer.Modules.Generic
{
    public class KmlImporter : IFileImporter
    {
        public const string Type = "kml";
        public DataSources Source => DataSources.Kml;

        public ImportClassification? Classify(string filepath)
        {
            var peeker = new FilePeeker(filepath);
            var lines = peeker.ReadLines(10);

            if (lines.All(x => !x.Contains("<kml")))
                return null;

            return new ImportClassification
            {
                Filepath = filepath,
                Filetype = Type,
                Source = Source,
                Datetype = DateRanges.Time,
            };
        }

        public List<FileExtraction> Extract(ArchiveIndex file)
        {
            var kml = new KmlFile(file.Filepath);
            var points = kml.GetPoints();

            var latSeries = new List<string>();
            var lonSeries = new List<string>();
            var altSeries = new List<string>();

            var minAltitude = int.MaxValue;
            var maxAltitude = int.MinValue;
            double distance = 0;

            GpsPoint? lastPoint = null;
            var calc = new GpsCalculator();

            foreach (var point in points)
            {
                latSeries.Add(point.Latitude.ToString(CultureInfo.InvariantCulture));
                lonSeries.Add(point.Longitude.ToString(CultureInfo.InvariantCulture));

                if (lastPoint != null)
                    distance += calc.HaversineDistance(lastPoint, point);

                if (point.Altitude.HasValue)
                {
                    var altitude = point.Altitude.Value;

                    if (minAltitude > altitude)
                        minAltitude = (int)altitude;

                    if (maxAltitude < altitude)
                        maxAltitude = (int)altitude;

                    altSeries.Add(altitude.ToString(CultureInfo.InvariantCulture));
                }

                lastPoint = point;
            }

            var series = new Dictionary<Parameters, IEnumerable<string>>
            {
                { Parameters.Latitude, latSeries },
                { Parameters.Longitude, lonSeries },
            };

            var attributes = new Dictionary<Parameters, string>
            {
                { Parameters.Distance, distance.ToString(CultureInfo.InvariantCulture) },
            };

            if (altSeries.Count > 0)
            {
                series.Add(Parameters.Altitude, altSeries);
                attributes.Add(
                    Parameters.MinAltitude,
                    minAltitude.ToString(CultureInfo.InvariantCulture)
                );
                attributes.Add(
                    Parameters.MaxAltitude,
                    maxAltitude.ToString(CultureInfo.InvariantCulture)
                );
            }

            var result = new FileExtraction(
                Source,
                file.Filepath,
                file.Date,
                DateRanges.Time,
                attributes,
                series
            );

            return [result];
        }
    }
}
