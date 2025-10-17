using System.Globalization;
using System.Text.Json;
using System.Xml;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Models.Files;
using TheFipster.ActivityAggregator.Domain.Models.Requests;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Domain.Vendor.Polar.Flow;
using TheFipster.ActivityAggregator.Importer.Features.Extraction.Components.Contracts;

namespace TheFipster.ActivityAggregator.Importer.Features.Extraction.Components;

public class PolarFlowSleepResultExtractor : IFileExtractor
{
    public DataSources Source => DataSources.PolarFlowSleepResult;
    public int ExtractorVersion => 1;

    public List<FileExtraction> Extract(ExtractionRequest file)
    {
        var json = File.ReadAllText(file.Filepath);
        var sleepResults =
            JsonSerializer.Deserialize<List<PolarFlowSleepResult>>(json)
            ?? throw new ArgumentException("Couldn't parse polar takeout sleep result.");

        var result = new List<FileExtraction>();

        foreach (var item in sleepResults)
        {
            var date = item.Night.Date;
            var extraction = new FileExtraction(
                DataSources.PolarFlowSleepResult,
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
            extraction.Attributes.Add(
                Parameters.SleepStart,
                start.ToString(DateHelper.SecondFormat)
            );
            extraction.Attributes.Add(Parameters.SleepEnd, end.ToString(DateHelper.SecondFormat));

            result.Add(extraction);
        }

        return result;
    }
}
