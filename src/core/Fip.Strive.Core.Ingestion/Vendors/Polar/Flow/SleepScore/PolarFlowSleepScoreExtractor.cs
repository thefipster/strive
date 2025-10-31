using System.Globalization;
using System.Text.Json;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Models;
using Fip.Strive.Core.Ingestion.Contracts;

namespace Fip.Strive.Core.Ingestion.Vendors.Polar.Flow.SleepScore;

public class PolarFlowSleepScoreExtractor : IFileExtractor
{
    public DataSources Source => DataSources.PolarFlowSleepScore;
    public int ExtractorVersion => 1;

    public List<FileExtraction> Extract(string filepath, DateTime? _ = null)
    {
        var json = File.ReadAllText(filepath);
        var sleepScores =
            JsonSerializer.Deserialize<List<PolarFlowSleepScore>>(json)
            ?? throw new ArgumentException("Couldn't parse polar takeout sleep score.");

        var result = new List<FileExtraction>();

        foreach (var item in sleepScores)
        {
            var date = item.Night.Date;
            var extraction = new FileExtraction(
                DataSources.PolarFlowSleepScore,
                filepath,
                date,
                DataKind.Day
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
