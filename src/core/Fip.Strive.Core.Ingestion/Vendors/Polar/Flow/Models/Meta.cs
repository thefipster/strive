using System.Text.Json.Serialization;

namespace Fip.Strive.Core.Ingestion.Vendors.Polar.Flow.Models;

public class Meta
{
    [JsonPropertyName("id")]
    public Id? Id { get; set; }

    [JsonPropertyName("type")]
    public int Type { get; set; }

    [JsonPropertyName("ownerId")]
    public string? OwnerId { get; set; }

    [JsonPropertyName("created")]
    public DateTimeOffset Created { get; set; }

    [JsonPropertyName("modified")]
    public DateTimeOffset Modified { get; set; }

    [JsonPropertyName("endTime")]
    public DateTimeOffset EndTime { get; set; }
}
