using System.Text.Json.Serialization;

namespace TheFipster.ActivityAggregator.Polar.Domain;

public class PolarTakeoutGenericPeriodSkinContact
{
    [JsonPropertyName("meta")]
    public Meta Meta { get; set; }

    [JsonPropertyName("data")]
    public SkinContactData Data { get; set; }
}

public class SkinContactData
{
    [JsonPropertyName("sourceDeviceId")]
    public string SourceDeviceId { get; set; }

    [JsonPropertyName("skinContactChanges")]
    public List<SkinContactChange> SkinContactChanges { get; set; }
}

public class SkinContactChange
{
    [JsonPropertyName("recordingTimeDeltaMilliseconds")]
    public string RecordingTimeDeltaMilliseconds { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("skinContact")]
    public bool? SkinContact { get; set; }
}
