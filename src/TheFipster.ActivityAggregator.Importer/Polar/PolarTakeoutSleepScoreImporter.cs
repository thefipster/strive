using System.Globalization;
using System.Text.Json;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Models.Files;
using TheFipster.ActivityAggregator.Domain.Models.Importing;
using TheFipster.ActivityAggregator.Domain.Models.Requests;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Importer.Abstractions;
using TheFipster.ActivityAggregator.Polar.Domain;

namespace TheFipster.ActivityAggregator.Importer.Polar;

public class PolarTakeoutSleepScoreImporter : IFileClassifier, IFileExtractor
{
    public DataSources Source => DataSources.PolarFlowSleepScore;
    public int ClassifierVersion => 1;
    public int ExtractorVersion => 1;

    private readonly HashSet<string> _required =
    [
        "night",
        "sleepScoreResult",
        "sleepScore",
        "sleepTimeOwnTargetScore",
        "sleepTimeRecommendationScore",
    ];

    public ImportClassification Classify(FileProbe probe)
    {
        var values = probe.JsonValues;

        if (values == null)
            throw new ClassificationException(probe.Filepath, Source, "Couldn't find valid json.");

        if (!_required.IsSubsetOf(values.Keys))
            throw new ClassificationException(
                probe.Filepath,
                Source,
                "Couldn't find required properties."
            );

        var date = values["night"];
        if (string.IsNullOrWhiteSpace(date))
            throw new ClassificationException(probe.Filepath, Source, "Couldn't find date value.");

        return new ImportClassification
        {
            Filepath = probe.Filepath,
            Source = Source,
            Datetime = DateTime.Parse(date),
            Datetype = DateRanges.AllTime,
        };
    }

    public List<FileExtraction> Extract(ExtractionRequest file)
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
