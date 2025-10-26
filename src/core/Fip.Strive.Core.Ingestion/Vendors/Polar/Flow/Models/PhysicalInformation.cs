using System.Text.Json.Serialization;

namespace Fip.Strive.Core.Ingestion.Vendors.Polar.Flow.Models;

public class PhysicalInformation
{
    [JsonPropertyName("dateTime")]
    public DateTimeOffset DateTime { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("trainingBackground")]
    public string? TrainingBackground { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("vo2Max")]
    public long? Vo2Max { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("maximumHeartRate")]
    public long? MaximumHeartRate { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("restingHeartRate")]
    public long? RestingHeartRate { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("aerobicThreshold")]
    public long? AerobicThreshold { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("anaerobicThreshold")]
    public long? AnaerobicThreshold { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("typicalDay")]
    public string? TypicalDay { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("sleepGoal")]
    public string? SleepGoal { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("functionalThresholdPower")]
    public long? FunctionalThresholdPower { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("sex")]
    public string? Sex { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("birthday")]
    public DateTime? Birthday { get; set; }
}
