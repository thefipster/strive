using System.Globalization;
using Fip.Strive.Core.Domain.Exceptions;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Components;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Models;
using Fip.Strive.Core.Ingestion.Contracts;
using Fip.Strive.Core.Ingestion.Models;

namespace Fip.Strive.Core.Ingestion.Vendors.Generic.Gpx
{
    public class GenericGpxExtractor : IFileExtractor
    {
        public DataSources Source => DataSources.Gpx;
        public int ExtractorVersion => 1;

        public List<FileExtraction> Extract(string filepath, DateTime? date = null)
        {
            var gpx = new GpxFile(filepath);
            var points = gpx.GetPoints();

            var latSeries = new List<string>();
            var lonSeries = new List<string>();
            var altSeries = new List<string>();
            var speedSeries = new List<string>();
            var timeSeries = new List<string>();

            var minAltitude = int.MaxValue;
            var maxAltitude = int.MinValue;
            double distance = 0;

            DateTime? start = null;

            GpsPoint? lastPoint = null;
            var calc = new GpsCalculator();

            foreach (var point in points)
            {
                latSeries.Add(point.Latitude.ToString(CultureInfo.InvariantCulture));
                lonSeries.Add(point.Longitude.ToString(CultureInfo.InvariantCulture));

                if (point.Speed.HasValue)
                    speedSeries.Add(point.Speed.Value.ToString(CultureInfo.InvariantCulture));

                if (point.Timestamp.HasValue)
                {
                    if (start == null)
                        start = point.Timestamp.Value;

                    timeSeries.Add(point.Timestamp.Value.ToString(DateHelper.SecondFormat));
                }

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

            if (start == null)
                start = date;

            if (start == null)
                throw new ExtractionException(filepath, "No timestamp was found or provided.");

            var result = new FileExtraction(Source, filepath, start.Value, DataKind.Session);

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
