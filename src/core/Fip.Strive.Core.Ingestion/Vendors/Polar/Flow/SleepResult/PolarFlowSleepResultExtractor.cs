using System.Globalization;
using System.Text.Json;
using System.Xml;
using Fip.Strive.Core.Domain.Exceptions;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Components;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Models;
using Fip.Strive.Core.Ingestion.Contracts;

namespace Fip.Strive.Core.Ingestion.Vendors.Polar.Flow.SleepResult;

public class PolarFlowSleepResultExtractor : IFileExtractor
{
    public DataSources Source => DataSources.PolarFlowSleepResult;
    public int ExtractorVersion => 1;

    public List<FileExtraction> Extract(string filepath, DateTime? _ = null)
    {
        var json = File.ReadAllText(filepath);
        var sleepResults =
            JsonSerializer.Deserialize<List<PolarFlowSleepResult>>(json)
            ?? throw new ArgumentException("Couldn't parse polar takeout sleep result.");

        var result = new List<FileExtraction>();

        foreach (var item in sleepResults)
        {
            var date = item.Night.Date;
            var extraction = new FileExtraction(
                DataSources.PolarFlowSleepResult,
                filepath,
                date,
                DataKind.Day
            );

            if (
                item.Evaluation == null
                || item.Evaluation.SleepSpan == null
                || item.Evaluation.AsleepDuration == null
                || item.Evaluation.Analysis == null
            )
            {
                throw new ExtractionException(filepath, "No evaluation found.");
            }

            var totalDuration = (int)XmlConvert.ToTimeSpan(item.Evaluation.SleepSpan).TotalSeconds;
            var sleepDuration = (int)
                XmlConvert.ToTimeSpan(item.Evaluation.AsleepDuration).TotalSeconds;
            var efficiency = item.Evaluation.Analysis.EfficiencyPercent;

            if (item.SleepResult == null || item.SleepResult.Hypnogram == null)
                throw new ExtractionException(filepath, "No sleep result found.");

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
