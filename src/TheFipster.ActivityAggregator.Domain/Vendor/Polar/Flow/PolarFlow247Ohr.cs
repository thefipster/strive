using System.Text.Json.Serialization;

namespace TheFipster.ActivityAggregator.Domain.Vendor.Polar.Flow;

public class PolarFlow247Ohr
{
    [JsonPropertyName("deviceDays")]
    public List<DeviceDay> DeviceDays { get; set; } = new();
}

public class DeviceDay
{
    [JsonPropertyName("userId")]
    public int UserId { get; set; }

    [JsonPropertyName("deviceId")]
    public string DeviceId { get; set; } = string.Empty;

    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("samples")]
    public List<HeartrateSample> Samples { get; set; } = new();
}

public class HeartrateSample
{
    [JsonPropertyName("heartRate")]
    public int HeartRate { get; set; }

    [JsonPropertyName("secondsFromDayStart")]
    public int SecondsFromDayStart { get; set; }

    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;
}
