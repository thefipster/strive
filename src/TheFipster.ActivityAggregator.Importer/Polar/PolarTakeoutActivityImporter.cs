using System.Globalization;
using System.Text.Json;
using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Models;
using TheFipster.ActivityAggregator.Domain.Models.Scanner;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Importer.Modules.Abstractions;
using TheFipster.ActivityAggregator.Polar.Domain;

namespace TheFipster.ActivityAggregator.Importer.Polar
{
    public class PolarTakeoutActivityImporter : IFileImporter
    {
        public DataSources Source => DataSources.PolarTakeoutActivity;
        public int ClassifierVersion => 1;
        public int ExtractorVersion => 1;

        private readonly HashSet<string> required = ["exportVersion", "date"];
        private readonly string[] found = ["samples", "summary"];

        public ImportClassification Classify(FileProbe probe)
        {
            var values = probe.JsonValues;

            if (values == null)
                throw new ClassificationException(
                    probe.Filepath,
                    Source,
                    "Couldn't find valid json."
                );

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
                ?? throw new ArgumentException(
                    $"Couldn't parse polar takeout activity file {file.Filepath}."
                );

            var results = new List<FileExtraction>();
            results = AppendSummaryAttributes(results, file, activity);
            results = AppendStepsSeries(results, file, activity);
            results = AppendMetsSeries(results, file, activity);

            if (results.Any())
                return results;

            throw new ExtractionException(file.Filepath, "Couldn't find any data.");
        }

        private List<FileExtraction> AppendMetsSeries(
            List<FileExtraction> results,
            ArchiveIndex file,
            PolarTakeoutActivity activity
        )
        {
            if (
                activity.Samples == null
                || activity.Samples.Mets == null
                || activity.Samples.Mets.Count == 0
            )
                return results;

            var metsSeries = new List<string>();
            var timestampSeries = new List<string>();

            foreach (var sample in activity.Samples.Mets)
            {
                var timestamp = activity.Date.AddSeconds(sample.LocalTime.TotalSeconds);
                timestampSeries.Add(timestamp.ToString(DateHelper.SecondFormat));
                metsSeries.Add(((int)sample.Value).ToString());
            }

            var series = FileExtraction.EmptySeries;
            series.Add(Parameters.Timestamp, timestampSeries);
            series.Add(Parameters.MetabolicRate, metsSeries);

            var result = new FileExtraction(
                Source,
                file.Filepath,
                activity.Date,
                DateRanges.Day,
                series
            );

            results.Add(result);
            return results;
        }

        private List<FileExtraction> AppendStepsSeries(
            List<FileExtraction> results,
            ArchiveIndex file,
            PolarTakeoutActivity activity
        )
        {
            if (
                activity.Samples == null
                || activity.Samples.Steps == null
                || activity.Samples.Steps.Count == 0
            )
                return results;

            var stepsSeries = new List<string>();
            var timestampSeries = new List<string>();

            foreach (var sample in activity.Samples.Steps)
            {
                var timestamp = activity.Date.AddSeconds(sample.LocalTime.TotalSeconds);
                timestampSeries.Add(timestamp.ToString(DateHelper.SecondFormat));
                stepsSeries.Add(((int)sample.Value).ToString());
            }

            var series = FileExtraction.EmptySeries;
            series.Add(Parameters.Timestamp, timestampSeries);
            series.Add(Parameters.Steps, stepsSeries);

            var result = new FileExtraction(
                Source,
                file.Filepath,
                activity.Date,
                DateRanges.Day,
                series
            );

            results.Add(result);
            return results;
        }

        private List<FileExtraction> AppendSummaryAttributes(
            List<FileExtraction> results,
            ArchiveIndex file,
            PolarTakeoutActivity activity
        )
        {
            if (activity.Summary != null)
            {
                var attributes = GetAttributes(activity.Summary);
                var result = new FileExtraction(
                    Source,
                    file.Filepath,
                    activity.Date,
                    DateRanges.Day,
                    attributes
                );
                results.Add(result);
            }

            return results;
        }

        private Dictionary<Parameters, string> GetAttributes(Summary summary)
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
    }
}
