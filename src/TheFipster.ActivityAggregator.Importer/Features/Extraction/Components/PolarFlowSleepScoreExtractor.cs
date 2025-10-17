using System.Globalization;
using System.Text.Json;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Files;
using TheFipster.ActivityAggregator.Domain.Models.Requests;
using TheFipster.ActivityAggregator.Domain.Vendor.Polar.Flow;
using TheFipster.ActivityAggregator.Importer.Features.Extraction.Components.Contracts;

namespace TheFipster.ActivityAggregator.Importer.Features.Extraction.Components;

public class PolarFlowSleepScoreExtractor : IFileExtractor
{
    public DataSources Source => DataSources.PolarFlowSleepScore;
    public int ExtractorVersion => 1;

    public List<FileExtraction> Extract(ExtractionRequest file)
    {
        var json = File.ReadAllText(file.Filepath);
        var sleepScores =
            JsonSerializer.Deserialize<List<PolarFlowSleepScore>>(json)
            ?? throw new ArgumentException("Couldn't parse polar takeout sleep score.");

        var result = new List<FileExtraction>();

        foreach (var item in sleepScores)
        {
            var date = item.Night.Date;
            var extraction = new FileExtraction(
                DataSources.PolarFlowSleepScore,
                file.Filepath,
                date,
                DateRanges.Day
            );

            var sleepScore = item.SleepScoreResult["sleepScore"];
            var continuityScore = item.SleepScoreResult["continuityScore"];
            var efficiencyScore = item.SleepScoreResult["efficiencyScore"];

            extraction.Attributes.Add(
                Parameters.SleepScore,
                sleepScore.ToString(CultureInfo.InvariantCulture)
            );
            extraction.Attributes.Add(
                Parameters.SleepContinuity,
                continuityScore.ToString(CultureInfo.InvariantCulture)
            );
            extraction.Attributes.Add(
                Parameters.SleepEfficiency,
                efficiencyScore.ToString(CultureInfo.InvariantCulture)
            );

            result.Add(extraction);
        }

        return result;
    }
}
