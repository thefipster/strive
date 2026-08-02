using System.Globalization;
using System.Text.Json;
using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.Contracts;
using Fip.Strive.Ingestion.Domain.Components;
using Fip.Strive.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Domain.Exceptions;
using Fip.Strive.Ingestion.Domain.Models;
using Microsoft.Extensions.Logging;

namespace Fip.Strive.Ingestion.Application.Vendors.Polar.Flow.Activity;

public class PolarFlowActivityExtractor(ILogger<PolarFlowActivityExtractor> logger) : IFileExtractor
{
    public DataSources Source => DataSources.PolarFlowActivity;
    public int ExtractorVersion => 1;

    public List<FileExtraction> Extract(string filepath, DateTime? date = null)
    {
        var activity = DeserializeActivity(filepath);
        var results = ExtractData(filepath, activity);

        if (results.Any())
            return results;

        throw new ExtractionException(filepath, "Couldn't find any data.");
    }

    private PolarFlowActivity DeserializeActivity(string filepath)
    {
        var json = File.ReadAllText(filepath);
        return JsonSerializer.Deserialize<PolarFlowActivity>(json)
            ?? throw new ArgumentException(
                $"Couldn't parse polar takeout activity file {filepath}."
            );
    }

    private List<FileExtraction> ExtractData(string filepath, PolarFlowActivity activity)
    {
        var results = new List<FileExtraction>();

        AppendSummaryAttributes(results, filepath, activity);
        AppendStepsSeries(results, filepath, activity);
        AppendMetsSeries(results, filepath, activity);

        return results;
    }

    private void AppendSummaryAttributes(
        List<FileExtraction> results,
        string filepath,
        PolarFlowActivity activity
    )
    {
        if (activity.Summary == null)
            return;

        var result = new FileExtraction(Source, filepath, activity.Date, DataKind.Day);

        result.AddAttribute(Parameters.Steps, activity.Summary.StepCount);
        result.AddAttribute(Parameters.Distance, activity.Summary.StepsDistance);
        result.AddAttribute(Parameters.Calories, activity.Summary.Calories);

        results.Add(result);
    }

    private void AppendStepsSeries(
        List<FileExtraction> results,
        string filepath,
        PolarFlowActivity activity
    )
    {
        try
        {
            var steps = EnsureSteps(activity);
            var series = CreateStepsSeries(activity.Date, filepath, steps);
            results.Add(series);
        }
        catch (InvalidDataException)
        {
            logger.LogInformation("No steps found in {Filepath}.", filepath);
        }
        catch (Exception e)
        {
            throw new ExtractionException(filepath, "Error while appending steps.", e);
        }
    }

    private void AppendMetsSeries(
        List<FileExtraction> results,
        string filepath,
        PolarFlowActivity activity
    )
    {
        try
        {
            var steps = EnsureMets(activity);
            var series = CreateMetsSeries(activity.Date, filepath, steps);
            results.Add(series);
        }
        catch (InvalidDataException)
        {
            logger.LogInformation("No mets found in {Filepath}.", filepath);
        }
        catch (Exception e)
        {
            throw new ExtractionException(filepath, "Error while appending mets.", e);
        }
    }

    private FileExtraction CreateStepsSeries(
        DateTime activityDate,
        string filepath,
        List<Met> samples
    )
    {
        var result = new FileExtraction(Source, filepath, activityDate, DataKind.Day);
        result.AddSeries(Parameters.Timestamp);
        result.AddSeries(Parameters.Steps);

        foreach (var sample in samples)
        {
            var timestamp = activityDate.AddSeconds(sample.LocalTime.TotalSeconds);
            result.Series[Parameters.Timestamp].Add(timestamp.ToString(DateHelper.SecondFormat));
            result.Series[Parameters.Steps].Add(((int)sample.Value).ToString());
        }

        return result;
    }

    private FileExtraction CreateMetsSeries(
        DateTime activityDate,
        string filepath,
        List<Met> samples
    )
    {
        var result = new FileExtraction(Source, filepath, activityDate, DataKind.Day);
        result.AddSeries(Parameters.Timestamp);
        result.AddSeries(Parameters.MetabolicRate);

        foreach (var sample in samples)
        {
            var timestamp = activityDate.AddSeconds(sample.LocalTime.TotalSeconds);
            result.Series[Parameters.Timestamp].Add(timestamp.ToString(DateHelper.SecondFormat));
            result.Series[Parameters.MetabolicRate].Add(((int)sample.Value).ToString());
        }

        return result;
    }

    private List<Met> EnsureMets(PolarFlowActivity activity)
    {
        if (activity.Samples?.Mets == null || activity.Samples.Mets.Count == 0)
            throw new InvalidDataException("No mets samples found.");

        return activity.Samples.Mets;
    }

    private List<Met> EnsureSteps(PolarFlowActivity activity)
    {
        if (activity.Samples?.Steps == null || activity.Samples.Steps.Count == 0)
            throw new InvalidDataException("No steps samples found.");

        return activity.Samples.Steps;
    }
}
