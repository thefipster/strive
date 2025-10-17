using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Files;
using TheFipster.ActivityAggregator.Domain.Models.Formats;
using TheFipster.ActivityAggregator.Domain.Models.Requests;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Importer.Features.Extraction.Components.Contracts;

namespace TheFipster.ActivityAggregator.Importer.Features.Extraction.Components;

public class GoogleFitbitStepsExtractor : IFileExtractor
{
    public int ExtractorVersion => 1;
    public DataSources Source => DataSources.GoogleFitbitSteps;

    public List<FileExtraction> Extract(ExtractionRequest file)
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
