using System.Globalization;
using System.Text.Json;
using Fipster.TrackMe.Domain;
using Fipster.TrackMe.Domain.Models;
using Fipster.TrackMe.Domain.Tools;
using Fipster.TrackMe.Importer.Modules.Abstractions;
using Fipster.TrackMe.Polar.Domain;

namespace Fipster.TrackMe.Importer.Polar
{
    public class PolarTakeoutActivityImporter : IFileImporter
    {
        public string Type => "polar_takeout_activity";
        public DataSources Source => DataSources.PolarTakeoutActivity;

        public ImportClassification? Classify(string filepath)
        {
            var peeker = new FilePeeker(filepath);

            var result = peeker.ReadChars(256);
            if (!result.Contains("\"stepCount\"") && !result.Contains("\"mets\""))
                return null;

            var date = peeker.ReadTokens("date");

            if (string.IsNullOrWhiteSpace(date))
                return null;

            return new ImportClassification
            {
                Filepath = filepath,
                Source = Source,
                Filetype = Type,
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
