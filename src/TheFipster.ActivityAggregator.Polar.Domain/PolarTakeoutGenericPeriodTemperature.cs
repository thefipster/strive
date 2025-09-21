using System.Text.Json.Serialization;

namespace TheFipster.ActivityAggregator.Polar.Domain;

public class PolarTakeoutGenericPeriodTemperature
{
    [JsonPropertyName("meta")]
    public Meta Meta { get; set; }

    [JsonPropertyName("data")]
    public TemperatureData Data { get; set; }
}

public class TemperatureData
{
    [JsonPropertyName("measurementType")]
    public string? MeasurementType { get; set; }

    [JsonPropertyName("sourceDeviceId")]
    public string? SourceDeviceId { get; set; }

    [JsonPropertyName("sensorLocation")]
    public string? SensorLocation { get; set; }

    [JsonPropertyName("temperatureMeasurementSamples")]
    public List<TemperatureMeasurementSample>? TemperatureMeasurementSamples { get; set; }
}

public class TemperatureMeasurementSample
{
    [JsonPropertyName("recordingTimeDeltaMilliseconds")]
    public string RecordingTimeDeltaMilliseconds { get; set; }

    [JsonPropertyName("temperatureCelsius")]
    public double TemperatureCelsius { get; set; }
}
