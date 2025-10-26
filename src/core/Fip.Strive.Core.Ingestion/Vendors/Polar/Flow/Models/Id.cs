using System.Text.Json.Serialization;

namespace Fip.Strive.Core.Ingestion.Vendors.Polar.Flow.Models;

public class Id
{
    [JsonPropertyName("startTimeSeconds")]
    public string? StartTimeSeconds { get; set; }

    [JsonPropertyName("restOfBytes")]
    public string? RestOfBytes { get; set; }
}
