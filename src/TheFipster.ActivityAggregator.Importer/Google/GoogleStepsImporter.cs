using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Formats;
using TheFipster.ActivityAggregator.Domain.Models;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Importer.Abstractions;
using TheFipster.ActivityAggregator.Importer.Modules.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Google;

public class GoogleStepsImporter()
    : GoogleCsvParser(DataSources.FitbitTakeoutSteps, DateRanges.Month, "timestamp,steps"),
        IFileImporter
{
    public List<FileExtraction> Extract(ArchiveIndex file)
    {
        var csv = new CsvFile(file.Filepath, ",");
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
                file.Filepath,
                day.Key,
                DateRanges.Day,
                attributes,
                series
            );
            results.Add(result);
        }

        return results;
    }
}
