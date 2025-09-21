using System.Text.Json.Serialization;

namespace TheFipster.ActivityAggregator.Polar.Domain;

public class PolarTakeoutGenericPeriodEcg
{
    [JsonPropertyName("meta")]
    public Meta Meta { get; set; }

    [JsonPropertyName("data")]
    public EcgData Data { get; set; }
}

public class EcgData
{
    [JsonPropertyName("sourceDeviceId")]
    public string SourceDeviceId { get; set; }

    [JsonPropertyName("testTime")]
    public string TestTime { get; set; }

    [JsonPropertyName("timeZoneOffset")]
    public int TimeZoneOffset { get; set; }

    [JsonPropertyName("averageHeartRateBpm")]
    public int AverageHeartRateBpm { get; set; }

    [JsonPropertyName("heartRateVariabilityMs")]
    public double HeartRateVariabilityMs { get; set; }

    [JsonPropertyName("heartRateVariabilityLevel")]
    public string HeartRateVariabilityLevel { get; set; }

    [JsonPropertyName("rriMs")]
    public double RriMs { get; set; }

    [JsonPropertyName("pulseArrivalTimeAtContractionMs")]
    public double PulseArrivalTimeAtContractionMs { get; set; }

    [JsonPropertyName("pulseArrivalTimeAtRelaxationMs")]
    public double PulseArrivalTimeAtRelaxationMs { get; set; }

    [JsonPropertyName("pulseTransitTimeQualityIndex")]
    public double PulseTransitTimeQualityIndex { get; set; }

    [JsonPropertyName("samples")]
    public List<EcgSample> Samples { get; set; }

    [JsonPropertyName("qualityMeasurements")]
    public List<QualityMeasurement> QualityMeasurements { get; set; }
}

public class QualityMeasurement
{
    [JsonPropertyName("recordingTimeDeltaMs")]
    public long RecordingTimeDeltaMs { get; set; }

    [JsonPropertyName("qualityLevel")]
    public string QualityLevel { get; set; }
}

public class EcgSample
{
    [JsonPropertyName("recordingTimeDeltaMs")]
    public long RecordingTimeDeltaMs { get; set; }

    [JsonPropertyName("amplitudeMv")]
    public double AmplitudeMv { get; set; }
}

public class Meta
{
    [JsonPropertyName("id")]
    public Id Id { get; set; }

    [JsonPropertyName("type")]
    public int Type { get; set; }

    [JsonPropertyName("ownerId")]
    public string OwnerId { get; set; }

    [JsonPropertyName("created")]
    public DateTimeOffset Created { get; set; }

    [JsonPropertyName("modified")]
    public DateTimeOffset Modified { get; set; }

    [JsonPropertyName("endTime")]
    public DateTimeOffset EndTime { get; set; }
}

public class Id
{
    [JsonPropertyName("startTimeSeconds")]
    public string StartTimeSeconds { get; set; }

    [JsonPropertyName("restOfBytes")]
    public string RestOfBytes { get; set; }
}
