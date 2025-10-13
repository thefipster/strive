using System.Text.Json.Serialization;

namespace TheFipster.ActivityAggregator.Domain.Vendor.Polar.Flow;

public class PolarFlowGenericPeriodSpo2
{
    [JsonPropertyName("meta")]
    public Meta? Meta { get; set; }

    [JsonPropertyName("data")]
    public Spo2Data? Data { get; set; }
}

public class Spo2Data
{
    [JsonPropertyName("recordingDevice")]
    public string? RecordingDevice { get; set; }

    [JsonPropertyName("testTime")]
    public string? TestTime { get; set; }

    [JsonPropertyName("timeZoneOffset")]
    public long TimeZoneOffset { get; set; }

    [JsonPropertyName("bloodOxygenPercent")]
    public long BloodOxygenPercent { get; set; }

    [JsonPropertyName("spo2Class")]
    public string? Spo2Class { get; set; }

    [JsonPropertyName("spo2ValueDeviationFromBaseline")]
    public string? Spo2ValueDeviationFromBaseline { get; set; }

    [JsonPropertyName("spo2QualityAveragePercent")]
    public double Spo2QualityAveragePercent { get; set; }

    [JsonPropertyName("averageHeartRateBpm")]
    public long AverageHeartRateBpm { get; set; }

    [JsonPropertyName("heartRateVariabilityMs")]
    public double HeartRateVariabilityMs { get; set; }

    [JsonPropertyName("spo2HrvDeviationFromBaseline")]
    public string? Spo2HrvDeviationFromBaseline { get; set; }
}
