using System.Text.Json.Serialization;

namespace Fip.Strive.Ingestion.Application.Vendors.TheFipsterApp.Bodyweight;

public class TheFipsterAppWeightEntry
{
    public double Weight { get; set; }
    public string Reason { get; set; } = string.Empty;

    [JsonPropertyName("Date")]
    public DateTimeOffset Timestamp { get; set; } = DateTime.UtcNow;
}
