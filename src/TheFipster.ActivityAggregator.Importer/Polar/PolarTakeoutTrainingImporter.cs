using System.Globalization;
using System.Text.Json;
using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Models;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Polar.Domain;
using TheFipster.ActivityAggregator.Importer.Modules.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Polar
{
    public class PolarTakeoutTrainingImporter : IFileImporter
    {
        public const string Type = "polar_takeout_training";
        public DataSources Source => DataSources.PolarTakeoutTraining;

        public ImportClassification? Classify(string filepath)
        {
            var peeker = new FilePeeker(filepath);

            var result = peeker.ReadChars(1024);
            if (!result.Contains("\"stopTime\""))
                return null;

            var date = peeker.ReadTokens("startTime");

            if (string.IsNullOrWhiteSpace(date))
                return null;

            return new ImportClassification
            {
                Filepath = filepath,
                Source = Source,
                Filetype = Type,
                Datetime = DateTime.Parse(date),
                Datetype = DateRanges.Time,
            };
        }

        public List<FileExtraction> Extract(ArchiveIndex file)
        {
            var json = File.ReadAllText(file.Filepath);
            var polarTraining =
                JsonSerializer.Deserialize<PolarTakeoutTraining>(json)
                ?? throw new ArgumentException("Couldn't parse polar takeout training.");

            var start = polarTraining.StartTime;
            var duration = polarTraining
                .Duration.Replace("PT", string.Empty)
                .Replace("S", string.Empty);

            var attributes = FileExtraction.EmptyAttributes;
            attributes.Add(
                Parameters.Distance,
                polarTraining.Distance.ToString(CultureInfo.InvariantCulture)
            );
            attributes.Add(Parameters.Duration, duration);
            attributes.Add(Parameters.StartTime, start.DateTime.ToString("s"));

            var result = new List<FileExtraction>();
            var attributeResult = new FileExtraction(
                Source,
                file.Filepath,
                start.UtcDateTime,
                DateRanges.Time,
                attributes
            );
            result.Add(attributeResult);

            var exercise = polarTraining.Exercises.FirstOrDefault();
            if (exercise == null)
                return result;

            attributes.Add(Parameters.Sport, exercise.Sport);

            var altitudeSeries = createSimpleSeries(exercise.Samples.Altitude, Parameters.Altitude);
            appendSeries(result, start, attributes, altitudeSeries, file);

            var distanceSeries = createSimpleSeries(exercise.Samples.Distance, Parameters.Distance);
            appendSeries(result, start, attributes, distanceSeries, file);

            var heartrateSeries = createSimpleSeries(
                exercise.Samples.HeartRate,
                Parameters.Heartrate
            );
            appendSeries(result, start, attributes, heartrateSeries, file);

            var temperatureSeries = createSimpleSeries(
                exercise.Samples.Temperature,
                Parameters.Temperature
            );
            appendSeries(result, start, attributes, temperatureSeries, file);

            var cadenceSeries = createSimpleSeries(exercise.Samples.Cadence, Parameters.Cadence);
            appendSeries(result, start, attributes, cadenceSeries, file);

            var speedSeries = createSimpleSeries(exercise.Samples.Speed, Parameters.Speed);
            appendSeries(result, start, attributes, speedSeries, file);

            var powerSeries = createPowerSeries(exercise.Samples.LeftPedalCrankBasedPower);
            appendSeries(result, start, attributes, powerSeries, file);

            var routeSeries = createRoute(exercise.Samples.RecordedRoute);
            appendSeries(result, start, attributes, routeSeries, file);

            var rrSeries = createRrSeries(exercise.Samples.Rr, start.DateTime);
            appendSeries(result, start, attributes, rrSeries, file);

            return result;
        }

        private void appendSeries(
            List<FileExtraction> result,
            DateTimeOffset start,
            Dictionary<Parameters, string> attributes,
            Dictionary<Parameters, IEnumerable<string>>? series,
            ArchiveIndex file
        )
        {
            if (series != null)
                result.Add(
                    new FileExtraction(
                        Source,
                        file.Filepath,
                        start.UtcDateTime,
                        DateRanges.Time,
                        attributes,
                        series
                    )
                );
        }

        private Dictionary<Parameters, IEnumerable<string>>? createRrSeries(
            List<RrSample> samples,
            DateTime date
        )
        {
            if (samples == null || samples.Count == 0)
                return null;

            var rr = samples.Select(x =>
                x.Duration.Replace("PT", string.Empty).Replace("S", string.Empty)
            );

            var series = FileExtraction.EmptySeries;
            series.Add(Parameters.Rr, rr);
            return series;
        }

        private Dictionary<Parameters, IEnumerable<string>>? createPowerSeries(
            List<IntSample> samples
        )
        {
            if (samples == null || samples.Count == 0)
                return null;

            var start = samples.First().DateTime;
            var powerSeries = samples.Select(x => x.CurrentPower.ToString());
            var durationSeries = samples.Select(x =>
                ((int)(x.DateTime - start).TotalSeconds).ToString()
            );

            var series = FileExtraction.EmptySeries;
            series.Add(Parameters.Duration, durationSeries);
            series.Add(Parameters.Power, powerSeries);

            return series;
        }

        private Dictionary<Parameters, IEnumerable<string>>? createRoute(List<GpsSample> samples)
        {
            if (samples == null || samples.Count == 0)
                return null;

            var durationSeries = new List<string>();
            var altitudeSeries = new List<string>();
            var latitudeSeries = new List<string>();
            var longitudeSeries = new List<string>();

            var start = samples.First().DateTime;

            foreach (var point in samples)
            {
                durationSeries.Add(((int)(point.DateTime - start).TotalSeconds).ToString());
                altitudeSeries.Add(point.Altitude.ToString(CultureInfo.InvariantCulture));
                latitudeSeries.Add(point.Latitude.ToString(CultureInfo.InvariantCulture));
                longitudeSeries.Add(point.Longitude.ToString(CultureInfo.InvariantCulture));
            }

            var series = FileExtraction.EmptySeries;
            series.Add(Parameters.Duration, durationSeries);
            series.Add(Parameters.Altitude, altitudeSeries);
            series.Add(Parameters.Latitude, latitudeSeries);
            series.Add(Parameters.Longitude, longitudeSeries);

            return series;
        }

        private Dictionary<Parameters, IEnumerable<string>>? createSimpleSeries(
            List<DoubleSample> samples,
            Parameters type
        )
        {
            if (
                samples == null
                || samples.Count == 0
                || samples.All(x => x.Value.HasValue == false)
            )
                return null;

            var start = samples.First().DateTime;
            var basicSeries = samples
                .Where(x => x.Value.HasValue)
                .Select(x => (x.Value ?? double.NaN).ToString());
            var durationSeries = samples.Select(x =>
                ((int)(x.DateTime - start).TotalSeconds).ToString()
            );

            var series = FileExtraction.EmptySeries;
            series.Add(Parameters.Duration, durationSeries);
            series.Add(type, basicSeries);
            return series;
        }
    }
}
