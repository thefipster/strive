using System.Globalization;
using System.Text.Json;
using System.Xml;
using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Models;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Polar.Domain;
using TheFipster.ActivityAggregator.Importer.Modules.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Polar;

public class PolarTakeoutSleepResultImporter : IFileImporter
{
    public string Type => "polar_takeout_sleep_result";

    public DataSources Source => DataSources.PolarTakeoutSleepResult;

    public ImportClassification? Classify(string filepath)
    {
        var peeker = new FilePeeker(filepath);

        var result = peeker.ReadChars(256);

        if (!result.Contains("\"evaluation\""))
            return null;

        if (!result.Contains("\"sleepSpan\""))
            return null;

        if (!result.Contains("\"asleepDuration\""))
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

            var totalDuration = (int)XmlConvert.ToTimeSpan(item.Evaluation.SleepSpan).TotalSeconds;
            var sleepDuration = (int)
                XmlConvert.ToTimeSpan(item.Evaluation.AsleepDuration).TotalSeconds;
            var efficiency = item.Evaluation.Analysis.EfficiencyPercent;
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
