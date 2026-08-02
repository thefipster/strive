using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.Contracts;
using Fip.Strive.Ingestion.Application.Models;
using Fip.Strive.Ingestion.Domain.Components;
using Fip.Strive.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Domain.Models;

namespace Fip.Strive.Ingestion.Application.Vendors.Google.Fitbit.Steps;

public class GoogleFitbitStepsExtractor : IFileExtractor
{
    public int ExtractorVersion => 1;
    public DataSources Source => DataSources.GoogleFitbitSteps;

    public List<FileExtraction> Extract(string filepath, DateTime? _ = null)
    {
        var csv = new CsvFile(filepath, ",");
        var data = csv.ReadLines().Skip(1).Select(x => (DateTime.Parse(x[0]), int.Parse(x[1])));
        var days = data.GroupBy(x => x.Item1.Date);
        var results = new List<FileExtraction>();

        foreach (var day in days)
        {
            var date = day.Key;
            var timestampSeries = new List<string>();
            var stepsSeries = new List<string>();
            var steps = 0;

            foreach (var sample in day)
            {
                var timestamp = date.AddSeconds(sample.Item1.TimeOfDay.TotalSeconds);
                timestampSeries.Add(timestamp.ToString(DateHelper.SecondFormat));
                stepsSeries.Add(sample.Item2.ToString());
                steps += sample.Item2;
            }

            var series = new Dictionary<Parameters, List<string>>()
            {
                { Parameters.Timestamp, timestampSeries },
                { Parameters.Steps, stepsSeries },
            };

            var attributes = new Dictionary<Parameters, string>()
            {
                { Parameters.Steps, steps.ToString() },
            };

            var result = new FileExtraction(
                Source,
                filepath,
                day.Key,
                DataKind.Day,
                attributes,
                series
            );
            results.Add(result);
        }

        return results;
    }
}
