using System.Text.Json.Serialization;
using Fip.Strive.Ingestion.Application.Vendors.Polar.Flow.Models;

namespace Fip.Strive.Ingestion.Application.Vendors.Polar.Flow.Activity;

public class PolarFlowActivity
{
    [JsonPropertyName("exportVersion")]
    public required string ExportVersion { get; set; }

    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("summary")]
    public Summary? Summary { get; set; }

    [JsonPropertyName("samples")]
    public StepSamples? Samples { get; set; }

    [JsonPropertyName("physicalInformation")]
    public PhysicalInformation? PhysicalInformation { get; set; }
}

public class StepSamples
{
    [JsonPropertyName("mets")]
    public List<Met>? Mets { get; set; }

    [JsonPropertyName("steps")]
    public List<Met>? Steps { get; set; }

    [JsonPropertyName("metSources")]
    public List<string>? MetSources { get; set; }
}

public class Met
{
    [JsonPropertyName("localTime")]
    public TimeSpan LocalTime { get; set; }

    [JsonPropertyName("value")]
    public double Value { get; set; }
}

public class Summary
{
    [JsonPropertyName("startTime")]
    public TimeSpan StartTime { get; set; }

    [JsonPropertyName("endTime")]
    public TimeSpan EndTime { get; set; }

    [JsonPropertyName("stepCount")]
    public long StepCount { get; set; }

    [JsonPropertyName("stepsDistance")]
    public double StepsDistance { get; set; }

    [JsonPropertyName("calories")]
    public long Calories { get; set; }

    [JsonPropertyName("sleepQuality")]
    public double SleepQuality { get; set; }

    [JsonPropertyName("sleepDuration")]
    public string? SleepDuration { get; set; }

    [JsonPropertyName("inactivityAlertCount")]
    public long InactivityAlertCount { get; set; }

    [JsonPropertyName("dailyMetMinutes")]
    public double DailyMetMinutes { get; set; }

    [JsonPropertyName("activityLevels")]
    public List<ActivityLevel>? ActivityLevels { get; set; }
}

public class ActivityLevel
{
    [JsonPropertyName("level")]
    public string? Level { get; set; }

    [JsonPropertyName("duration")]
    public string? Duration { get; set; }
}
