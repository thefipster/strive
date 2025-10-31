using System.Globalization;
using Fip.Strive.Core.Domain.Exceptions;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Components;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Models;
using Fip.Strive.Core.Ingestion.Contracts;
using Fip.Strive.Core.Ingestion.Models;

namespace Fip.Strive.Core.Ingestion.Vendors.Generic.Kml
{
    public class GenericKmlExtractor : IFileExtractor
    {
        public DataSources Source => DataSources.Kml;
        public int ExtractorVersion => 1;

        public List<FileExtraction> Extract(string filepath, DateTime? date = null)
        {
            if (date == null)
                throw new ExtractionException(filepath, "Date is required for KML files.");

            var kml = new KmlFile(filepath);
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
                filepath,
                date.Value,
                DataKind.Session,
                attributes,
                series
            );

            return [result];
        }
    }
}
