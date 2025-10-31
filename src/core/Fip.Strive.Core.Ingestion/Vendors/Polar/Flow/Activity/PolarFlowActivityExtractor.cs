using System.Globalization;
using System.Text.Json;
using Fip.Strive.Core.Domain.Exceptions;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Components;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Models;
using Fip.Strive.Core.Ingestion.Contracts;

namespace Fip.Strive.Core.Ingestion.Vendors.Polar.Flow.Activity
{
    public class PolarFlowActivityExtractor : IFileExtractor
    {
        public DataSources Source => DataSources.PolarFlowActivity;
        public int ExtractorVersion => 1;

        public List<FileExtraction> Extract(string filepath, DateTime? date = null)
        {
            var json = File.ReadAllText(filepath);
            var activity =
                JsonSerializer.Deserialize<PolarFlowActivity>(json)
                ?? throw new ArgumentException(
                    $"Couldn't parse polar takeout activity file {filepath}."
                );

            var results = new List<FileExtraction>();
            results = AppendSummaryAttributes(results, filepath, activity);
            results = AppendStepsSeries(results, filepath, activity);
            results = AppendMetsSeries(results, filepath, activity);

            if (results.Any())
                return results;

            throw new ExtractionException(filepath, "Couldn't find any data.");
        }

        private List<FileExtraction> AppendMetsSeries(
            List<FileExtraction> results,
            string filepath,
            PolarFlowActivity activity
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

            var result = new FileExtraction(Source, filepath, activity.Date, DataKind.Day, series);

            results.Add(result);
            return results;
        }

        private List<FileExtraction> AppendStepsSeries(
            List<FileExtraction> results,
            string filepath,
            PolarFlowActivity activity
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

            var result = new FileExtraction(Source, filepath, activity.Date, DataKind.Day, series);

            results.Add(result);
            return results;
        }

        private List<FileExtraction> AppendSummaryAttributes(
            List<FileExtraction> results,
            string filepath,
            PolarFlowActivity activity
        )
        {
            if (activity.Summary != null)
            {
                var attributes = GetAttributes(activity.Summary);
                var result = new FileExtraction(
                    Source,
                    filepath,
                    activity.Date,
                    DataKind.Day,
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
