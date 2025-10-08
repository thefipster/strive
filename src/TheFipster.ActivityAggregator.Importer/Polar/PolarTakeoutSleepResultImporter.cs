using System.Globalization;
using System.Text.Json;
using System.Xml;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Models.Files;
using TheFipster.ActivityAggregator.Domain.Models.Importing;
using TheFipster.ActivityAggregator.Domain.Models.Requests;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Importer.Abstractions;
using TheFipster.ActivityAggregator.Polar.Domain;

namespace TheFipster.ActivityAggregator.Importer.Polar;

public class PolarTakeoutSleepResultImporter : IFileClassifier, IFileExtractor
{
    public DataSources Source => DataSources.PolarTakeoutSleepResult;
    public int ClassifierVersion => 1;
    public int ExtractorVersion => 1;

    private readonly HashSet<string> _required =
    [
        "night",
        "evaluation",
        "sleepType",
        "sleepSpan",
        "asleepDuration",
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
        var sleepResults =
            JsonSerializer.Deserialize<List<PolarTakeoutSleepResult>>(json)
            ?? throw new ArgumentException("Couldn't parse polar takeout sleep result.");

        var result = new List<FileExtraction>();

        foreach (var item in sleepResults)
        {
            var date = item.Night.Date;
            var extraction = new FileExtraction(
                DataSources.PolarTakeoutSleepResult,
                file.Filepath,
                date,
                DateRanges.Day
            );

            if (
                item.Evaluation == null
                || item.Evaluation.SleepSpan == null
                || item.Evaluation.AsleepDuration == null
                || item.Evaluation.Analysis == null
            )
            {
                throw new ExtractionException(file.Filepath, "No evaluation found.");
            }

            var totalDuration = (int)XmlConvert.ToTimeSpan(item.Evaluation.SleepSpan).TotalSeconds;
            var sleepDuration = (int)
                XmlConvert.ToTimeSpan(item.Evaluation.AsleepDuration).TotalSeconds;
            var efficiency = item.Evaluation.Analysis.EfficiencyPercent;

            if (item.SleepResult == null || item.SleepResult.Hypnogram == null)
                throw new ExtractionException(file.Filepath, "No sleep result found.");

            var start = item.SleepResult.Hypnogram.SleepStart.DateTime;
            var end = item.SleepResult.Hypnogram.SleepEnd.DateTime;

            extraction.Attributes.Add(Parameters.SleepDuration, totalDuration.ToString());
            extraction.Attributes.Add(Parameters.AsleepDuration, sleepDuration.ToString());
            extraction.Attributes.Add(
                Parameters.SleepPercentage,
                efficiency.ToString(CultureInfo.InvariantCulture)
            );
            extraction.Attributes.Add(Parameters.SleepStart, start.ToString("s"));
            extraction.Attributes.Add(Parameters.SleepEnd, end.ToString("s"));

            result.Add(extraction);
        }

        return result;
    }
}
