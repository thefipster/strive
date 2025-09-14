using System.Globalization;
using System.Text.Json;
using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Models;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Polar.Domain;
using TheFipster.ActivityAggregator.Importer.Modules.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Polar;

public class PolarTakeoutSleepScoreImporter : IFileImporter
{
    public string Type => "polar_takeout_sleep_score";

    public DataSources Source => DataSources.PolarTakeoutSleepScore;

    public ImportClassification? Classify(string filepath)
    {
        var peeker = new FilePeeker(filepath);

        var result = peeker.ReadChars(256);

        if (!result.Contains("\"sleepScoreResult\""))
            return null;

        if (!result.Contains("\"sleepTimeOwnTargetScore\""))
            return null;

        if (!result.Contains("\"sleepTimeRecommendationScore\""))
            return null;

        var date = peeker.ReadTokens("night");
        if (string.IsNullOrWhiteSpace(date))
            return null;

        return new ImportClassification
        {
            Filepath = filepath,
            Source = Source,
            Filetype = Type,
            Datetime = DateTime.Parse(date),
            Datetype = DateRanges.AllTime,
        };
    }

    public List<FileExtraction> Extract(ArchiveIndex file)
    {
        var json = File.ReadAllText(file.Filepath);
        var sleepScores =
            JsonSerializer.Deserialize<List<PolarTakeoutSleepScore>>(json)
            ?? throw new ArgumentException("Couldn't parse polar takeout sleep score.");

        var result = new List<FileExtraction>();

        foreach (var item in sleepScores)
        {
            var date = item.Night.Date;
            var extraction = new FileExtraction(
                DataSources.PolarTakeoutSleepScore,
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
