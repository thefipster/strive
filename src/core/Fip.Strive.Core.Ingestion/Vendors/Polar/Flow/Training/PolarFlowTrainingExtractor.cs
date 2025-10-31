using System.Globalization;
using System.Text.Json;
using Fip.Strive.Core.Domain.Exceptions;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Components;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Models;
using Fip.Strive.Core.Ingestion.Contracts;

namespace Fip.Strive.Core.Ingestion.Vendors.Polar.Flow.Training
{
    public class PolarFlowTrainingExtractor : IFileExtractor
    {
        public DataSources Source => DataSources.PolarFlowTraining;
        public int ExtractorVersion => 1;

        public List<FileExtraction> Extract(string filepath, DateTime? date = null)
        {
            var json = File.ReadAllText(filepath);
            var polarTraining =
                JsonSerializer.Deserialize<PolarFlowTraining>(json)
                ?? throw new ArgumentException("Couldn't parse polar takeout training.");

            var start = polarTraining.StartTime;

            if (polarTraining.Duration == null)
                throw new ExtractionException(filepath, "No duration found.");

            var duration = polarTraining
                .Duration.Replace("PT", string.Empty)
                .Replace("S", string.Empty);

            var attributes = FileExtraction.EmptyAttributes;
            attributes.Add(
                Parameters.Distance,
                polarTraining.Distance.ToString(CultureInfo.InvariantCulture)
            );
            attributes.Add(Parameters.Duration, duration);
            attributes.Add(Parameters.StartTime, start.DateTime.ToString(DateHelper.SecondFormat));

            var result = new List<FileExtraction>();
            var attributeResult = new FileExtraction(
                Source,
                filepath,
                start.UtcDateTime,
                DataKind.Session,
                attributes
            );
            result.Add(attributeResult);

            var exercise = polarTraining.Exercises.FirstOrDefault();
            if (exercise == null)
                return result;

            if (exercise.Sport == null)
                throw new ExtractionException(filepath, "No sport found.");

            attributes.Add(Parameters.Sport, exercise.Sport);

            if (exercise.Samples == null)
                return result;

            var altitudeSeries = CreateSimpleSeries(exercise.Samples.Altitude, Parameters.Altitude);
            AppendSeries(result, start, attributes, altitudeSeries, filepath);

            var distanceSeries = CreateSimpleSeries(exercise.Samples.Distance, Parameters.Distance);
            AppendSeries(result, start, attributes, distanceSeries, filepath);

            var heartrateSeries = CreateSimpleSeries(
                exercise.Samples.HeartRate,
                Parameters.Heartrate
            );
            AppendSeries(result, start, attributes, heartrateSeries, filepath);

            var temperatureSeries = CreateSimpleSeries(
                exercise.Samples.Temperature,
                Parameters.Temperature
            );
            AppendSeries(result, start, attributes, temperatureSeries, filepath);

            var cadenceSeries = CreateSimpleSeries(exercise.Samples.Cadence, Parameters.Cadence);
            AppendSeries(result, start, attributes, cadenceSeries, filepath);

            var speedSeries = CreateSimpleSeries(exercise.Samples.Speed, Parameters.Speed);
            AppendSeries(result, start, attributes, speedSeries, filepath);

            var powerSeries = CreatePowerSeries(exercise.Samples.LeftPedalCrankBasedPower);
            AppendSeries(result, start, attributes, powerSeries, filepath);

            var routeSeries = CreateRoute(exercise.Samples.RecordedRoute);
            AppendSeries(result, start, attributes, routeSeries, filepath);

            var rrSeries = CreateRrSeries(exercise.Samples.Rr);
            AppendSeries(result, start, attributes, rrSeries, filepath);
            return result;
        }

        private void AppendSeries(
            List<FileExtraction> result,
            DateTimeOffset start,
            Dictionary<Parameters, string> attributes,
            Dictionary<Parameters, List<string>>? series,
            string filepath
        )
        {
            if (series != null)
            {
                result.Add(
                    new FileExtraction(
                        Source,
                        filepath,
                        start.UtcDateTime,
                        DataKind.Session,
                        attributes,
                        series
                    )
                );
            }
        }

        private Dictionary<Parameters, List<string>>? CreateRrSeries(List<RrSample>? samples)
        {
            if (samples == null || samples.Count == 0)
                return null;

            var rr = samples
                .Where(x => x.Duration != null)
                .Select(x => x.Duration!.Replace("PT", string.Empty).Replace("S", string.Empty))
                .ToList();

            var series = FileExtraction.EmptySeries;
            series.Add(Parameters.Rr, rr);
            return series;
        }

        private Dictionary<Parameters, List<string>>? CreatePowerSeries(List<IntSample>? samples)
        {
            if (samples == null || samples.Count == 0)
                return null;

            var powerSeries = samples.Select(x => x.CurrentPower.ToString()).ToList();
            var timestampSeries = samples
                .Select(x => x.DateTime.ToString(DateHelper.SecondFormat))
                .ToList();

            var series = FileExtraction.EmptySeries;
            series.Add(Parameters.Timestamp, timestampSeries);
            series.Add(Parameters.Power, powerSeries);

            return series;
        }

        private Dictionary<Parameters, List<string>>? CreateRoute(List<GpsSample>? samples)
        {
            if (samples == null || samples.Count == 0)
                return null;

            var timestampSeries = new List<string>();
            var altitudeSeries = new List<string>();
            var latitudeSeries = new List<string>();
            var longitudeSeries = new List<string>();

            foreach (var point in samples)
            {
                timestampSeries.Add(point.DateTime.ToString(DateHelper.SecondFormat));
                altitudeSeries.Add(point.Altitude.ToString(CultureInfo.InvariantCulture));
                latitudeSeries.Add(point.Latitude.ToString(CultureInfo.InvariantCulture));
                longitudeSeries.Add(point.Longitude.ToString(CultureInfo.InvariantCulture));
            }

            var series = FileExtraction.EmptySeries;
            series.Add(Parameters.Timestamp, timestampSeries);
            series.Add(Parameters.Altitude, altitudeSeries);
            series.Add(Parameters.Latitude, latitudeSeries);
            series.Add(Parameters.Longitude, longitudeSeries);

            return series;
        }

        private Dictionary<Parameters, List<string>>? CreateSimpleSeries(
            List<DoubleSample>? samples,
            Parameters type
        )
        {
            if (
                samples == null
                || samples.Count == 0
                || samples.All(x => x.Value.HasValue == false)
            )
                return null;

            var basicSeries = samples
                .Where(x => x.Value.HasValue)
                .Select(x => (x.Value ?? double.NaN).ToString(CultureInfo.InvariantCulture))
                .ToList();
            var timestampSeries = samples
                .Select(x => x.DateTime.ToString(DateHelper.SecondFormat))
                .ToList();

            var series = FileExtraction.EmptySeries;
            series.Add(Parameters.Timestamp, timestampSeries);
            series.Add(type, basicSeries);
            return series;
        }
    }
}
