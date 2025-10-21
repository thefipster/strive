using System.Globalization;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Files;
using TheFipster.ActivityAggregator.Domain.Models.Formats;
using TheFipster.ActivityAggregator.Domain.Models.Requests;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Importer.Features.Extraction.Components.Contracts;

namespace TheFipster.ActivityAggregator.Importer.Features.Extraction.Components
{
    public class GenericKmlExtractor : IFileExtractor
    {
        public DataSources Source => DataSources.Kml;
        public int ExtractorVersion => 1;

        public List<FileExtraction> Extract(ExtractionRequest file)
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

            var series = new Dictionary<Parameters, List<string>>
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
