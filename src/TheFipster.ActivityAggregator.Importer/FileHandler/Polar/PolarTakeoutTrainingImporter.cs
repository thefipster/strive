using System.Globalization;
using System.Text.Json;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Models.Files;
using TheFipster.ActivityAggregator.Domain.Models.Importing;
using TheFipster.ActivityAggregator.Domain.Models.Requests;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Domain.Vendor.Polar.Flow;
using TheFipster.ActivityAggregator.Importer.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Polar
{
    public class PolarTakeoutTrainingImporter : IFileClassifier, IFileExtractor
    {
        public DataSources Source => DataSources.PolarFlowTraining;
        public int ClassifierVersion => 1;
        public int ExtractorVersion => 1;

        private readonly HashSet<string> _required =
        [
            "exportVersion",
            "startTime",
            "stopTime",
            "duration",
        ];

        public ImportClassification Classify(FileProbe probe)
        {
            var values = probe.JsonValues;

            if (values == null)
                throw new ClassificationException(
                    probe.Filepath,
                    Source,
                    "Couldn't find valid json."
                );

            if (!_required.IsSubsetOf(values.Keys))
                throw new ClassificationException(
                    probe.Filepath,
                    Source,
                    "Couldn't find required properties."
                );

            var date = values["startTime"];
            if (string.IsNullOrWhiteSpace(date))
                throw new ClassificationException(
                    probe.Filepath,
                    Source,
                    "Couldn't find date value."
                );

            return new ImportClassification
            {
                Filepath = probe.Filepath,
                Source = Source,
                Datetime = DateTime.Parse(date),
                Datetype = DateRanges.Time,
            };
        }

        public List<FileExtraction> Extract(ExtractionRequest file)
        {
            var json = File.ReadAllText(file.Filepath);
            var polarTraining =
                JsonSerializer.Deserialize<PolarFlowTraining>(json)
                ?? throw new ArgumentException("Couldn't parse polar takeout training.");

            var start = polarTraining.StartTime;

            if (polarTraining.Duration == null)
                throw new ExtractionException(file.Filepath, "No duration found.");

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
                file.Filepath,
                start.UtcDateTime,
                DateRanges.Time,
                attributes
            );
            result.Add(attributeResult);

            var exercise = polarTraining.Exercises.FirstOrDefault();
            if (exercise == null)
                return result;

            if (exercise.Sport == null)
                throw new ExtractionException(file.Filepath, "No sport found.");

            attributes.Add(Parameters.Sport, exercise.Sport);

            if (exercise.Samples == null)
                return result;

            var altitudeSeries = CreateSimpleSeries(exercise.Samples.Altitude, Parameters.Altitude);
            AppendSeries(result, start, attributes, altitudeSeries, file);

            var distanceSeries = CreateSimpleSeries(exercise.Samples.Distance, Parameters.Distance);
            AppendSeries(result, start, attributes, distanceSeries, file);

            var heartrateSeries = CreateSimpleSeries(
                exercise.Samples.HeartRate,
                Parameters.Heartrate
            );
            AppendSeries(result, start, attributes, heartrateSeries, file);

            var temperatureSeries = CreateSimpleSeries(
                exercise.Samples.Temperature,
                Parameters.Temperature
            );
            AppendSeries(result, start, attributes, temperatureSeries, file);

            var cadenceSeries = CreateSimpleSeries(exercise.Samples.Cadence, Parameters.Cadence);
            AppendSeries(result, start, attributes, cadenceSeries, file);

            var speedSeries = CreateSimpleSeries(exercise.Samples.Speed, Parameters.Speed);
            AppendSeries(result, start, attributes, speedSeries, file);

            var powerSeries = CreatePowerSeries(exercise.Samples.LeftPedalCrankBasedPower);
            AppendSeries(result, start, attributes, powerSeries, file);

            var routeSeries = CreateRoute(exercise.Samples.RecordedRoute);
            AppendSeries(result, start, attributes, routeSeries, file);

            var rrSeries = CreateRrSeries(exercise.Samples.Rr);
            AppendSeries(result, start, attributes, rrSeries, file);
            return result;
        }

        private void AppendSeries(
            List<FileExtraction> result,
            DateTimeOffset start,
            Dictionary<Parameters, string> attributes,
            Dictionary<Parameters, List<string>>? series,
            ExtractionRequest file
        )
        {
            if (series != null)
            {
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
