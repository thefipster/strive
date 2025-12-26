using System.Text.Json;
using System.Xml;
using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.Contracts;
using Fip.Strive.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Domain.Models;

namespace Fip.Strive.Ingestion.Application.Vendors.Polar.Flow.SleepResult;

public class PolarFlowSleepResultExtractor : IFileExtractor
{
    public DataSources Source => DataSources.PolarFlowSleepResult;
    public int ExtractorVersion => 1;

    public List<FileExtraction> Extract(string filepath, DateTime? _ = null)
    {
        var sleepResults = DeserializeSleepResults(filepath);

        var result = new List<FileExtraction>();

        foreach (var item in sleepResults)
        {
            if (
                item.Evaluation == null
                || item.Evaluation.SleepSpan == null
                || item.Evaluation.AsleepDuration == null
                || item.Evaluation.Analysis == null
            )
            {
                continue;
            }

            var extraction = new FileExtraction(
                DataSources.PolarFlowSleepResult,
                filepath,
                item.Night.Date,
                DataKind.Day
            );

            extraction.AddAttribute(
                Parameters.SleepDuration,
                (int)XmlConvert.ToTimeSpan(item.Evaluation.SleepSpan).TotalSeconds
            );

            extraction.AddAttribute(
                Parameters.AsleepDuration,
                (int)XmlConvert.ToTimeSpan(item.Evaluation.AsleepDuration).TotalSeconds
            );

            extraction.AddAttribute(
                Parameters.SleepPercentage,
                item.Evaluation.Analysis.EfficiencyPercent
            );

            if (item.SleepResult == null || item.SleepResult.Hypnogram == null)
            {
                result.Add(extraction);
                continue;
            }

            extraction.AddAttribute(
                Parameters.SleepStart,
                item.SleepResult.Hypnogram.SleepStart.DateTime.ToUniversalTime()
            );
            extraction.AddAttribute(
                Parameters.SleepEnd,
                item.SleepResult.Hypnogram.SleepEnd.DateTime.ToUniversalTime()
            );

            result.Add(extraction);
        }

        return result;
    }

    private static List<PolarFlowSleepResult> DeserializeSleepResults(string filepath)
    {
        var json = File.ReadAllText(filepath);
        var sleepResults =
            JsonSerializer.Deserialize<List<PolarFlowSleepResult>>(json)
            ?? throw new ArgumentException("Couldn't parse polar takeout sleep result.");
        return sleepResults;
    }
}
