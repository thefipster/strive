using System.Globalization;
using System.Text.Json;
using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Models;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Importer.Modules.Abstractions;
using TheFipster.ActivityAggregator.Polar.Domain;

namespace TheFipster.ActivityAggregator.Importer.Polar
{
    public class PolarTakeoutActivityImporter : IFileImporter
    {
        public DataSources Source => DataSources.PolarTakeoutActivity;

        private readonly HashSet<string> required = ["exportVersion", "date"];
        private readonly string[] found = ["samples", "summary"];

        public ImportClassification Classify(FileProbe probe)
        {
            var values = probe.GetJsonPropertiesWithValues();

            if (!required.IsSubsetOf(values.Keys))
                throw new ClassificationException(
                    probe.Filepath,
                    Source,
                    "Couldn't find required properties."
                );

            if (!values.Keys.Contains(found[0]) && !values.Keys.Contains(found[1]))
                throw new ClassificationException(
                    probe.Filepath,
                    Source,
                    "Couldn't find any needed properties."
                );

            var date = values["date"];

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
                Datetype = DateRanges.Day,
            };
        }

        public List<FileExtraction> Extract(ArchiveIndex file)
        {
            var json = File.ReadAllText(file.Filepath);
            var activity =
                JsonSerializer.Deserialize<PolarTakeoutActivity>(json)
                ?? throw new ArgumentException("Couldn't parse polar takeout activity.");

            var date = activity.Date;
            var result = new FileExtraction(Source, file.Filepath, date, DateRanges.Day);

            if (activity.Summary != null)
                result.Attributes = getAttributes(activity.Summary);

            if (
                activity.Samples != null
                && activity.Samples.Steps != null
                && activity.Samples.Steps.Count > 0
            )
            {
                result.Series = createStepsSeries(activity.Samples.Steps);
            }

            if (result.Attributes.Any() || result.Series.Any())
            {
                return [result];
            }

            throw new ArgumentException("Polar takeout activity yielded to data.");
        }

        private Dictionary<Parameters, string> getAttributes(Summary summary)
        {
            var attributes = FileExtraction.EmptyAttributes;

            attributes.Add(Parameters.Steps, summary.StepCount.ToString());
            attributes.Add(
                Parameters.Distance,
                summary.StepsDistance.ToString(CultureInfo.InvariantCulture)
            );
            attributes.Add(Parameters.Calories, summary.Calories.ToString());

            return attributes;
        }

        private Dictionary<Parameters, IEnumerable<string>> createStepsSeries(List<Met> samples)
        {
            var durationSeries = new List<string>();
            var stepsSeries = new List<string>();

            foreach (var sample in samples)
            {
                durationSeries.Add(((int)sample.LocalTime.TotalSeconds).ToString());
                stepsSeries.Add(((int)sample.Value).ToString());
            }

            var series = FileExtraction.EmptySeries;
            series.Add(Parameters.Duration, durationSeries);
            series.Add(Parameters.Steps, stepsSeries);

            return series;
        }
    }
}
