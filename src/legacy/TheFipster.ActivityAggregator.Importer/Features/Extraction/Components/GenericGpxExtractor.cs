using System.Globalization;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Files;
using TheFipster.ActivityAggregator.Domain.Models.Formats;
using TheFipster.ActivityAggregator.Domain.Models.Requests;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Importer.Features.Extraction.Components.Contracts;

namespace TheFipster.ActivityAggregator.Importer.Features.Extraction.Components
{
    public class GenericGpxExtractor : IFileExtractor
    {
        public DataSources Source => DataSources.Gpx;
        public int ExtractorVersion => 1;

        public List<FileExtraction> Extract(ExtractionRequest file)
        {
            var gpx = new GpxFile(file.Filepath);
            var points = gpx.GetPoints();

            var latSeries = new List<string>();
            var lonSeries = new List<string>();
            var altSeries = new List<string>();
            var speedSeries = new List<string>();
            var timeSeries = new List<string>();

            var minAltitude = int.MaxValue;
            var maxAltitude = int.MinValue;
            double distance = 0;

            GpsPoint? lastPoint = null;
            var calc = new GpsCalculator();

            foreach (var point in points)
            {
                latSeries.Add(point.Latitude.ToString(CultureInfo.InvariantCulture));
                lonSeries.Add(point.Longitude.ToString(CultureInfo.InvariantCulture));

                if (point.Speed.HasValue)
                    speedSeries.Add(point.Speed.Value.ToString(CultureInfo.InvariantCulture));

                if (point.Timestamp.HasValue)
                    timeSeries.Add(point.Timestamp.Value.ToString(DateHelper.SecondFormat));

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

            var result = new FileExtraction(Source, file.Filepath, file.Date, DateRanges.Time);

            result.Attributes.Add(
                Parameters.Distance,
                distance.ToString(CultureInfo.InvariantCulture)
            );

            result.Series.Add(Parameters.Latitude, latSeries);
            result.Series.Add(Parameters.Longitude, lonSeries);

            if (altSeries.Count > 0)
            {
                result.Series.Add(Parameters.Altitude, altSeries);

                result.Attributes.Add(
                    Parameters.MinAltitude,
                    minAltitude.ToString(CultureInfo.InvariantCulture)
                );
                result.Attributes.Add(
                    Parameters.MaxAltitude,
                    maxAltitude.ToString(CultureInfo.InvariantCulture)
                );
            }

            if (speedSeries.Count > 0)
                result.Series.Add(Parameters.Speed, speedSeries);

            if (timeSeries.Count > 0)
                result.Series.Add(Parameters.Timestamp, timeSeries);

            return [result];
        }
    }
}
