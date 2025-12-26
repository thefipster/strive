using System.Text.Json.Serialization;
using Fip.Strive.Core.Application.Converter;
using Fip.Strive.Ingestion.Application.Vendors.Polar.Flow.Models;

namespace Fip.Strive.Ingestion.Application.Vendors.Polar.Flow.Calendar;

public class PolarFlowCalendarItems
{
    [JsonPropertyName("exportVersion")]
    public string? ExportVersion { get; set; }

    [JsonPropertyName("perceivedRecovery")]
    public List<object>? PerceivedRecovery { get; set; }

    [JsonPropertyName("physicalInformations")]
    public List<PhysicalInformation>? PhysicalInformations { get; set; }

    [JsonPropertyName("notes")]
    public List<Note>? Notes { get; set; }

    [JsonPropertyName("feelings")]
    public List<Feeling>? Feelings { get; set; }

    [JsonPropertyName("weights")]
    public List<Weight>? Weights { get; set; }

    [JsonPropertyName("feedbacks")]
    public List<object>? Feedbacks { get; set; }
}

public class Feeling
{
    [JsonPropertyName("value")]
    public string? Value { get; set; }

    [JsonPropertyName("date")]
    public DateTimeOffset Date { get; set; }
}

public class Note
{
    [JsonPropertyName("note")]
    public string? NoteNote { get; set; }

    [JsonPropertyName("date")]
    public DateTimeOffset Date { get; set; }
}

public class Weight
{
    [JsonPropertyName("weight")]
    public double Value { get; set; }

    [JsonPropertyName("date")]
    [JsonConverter(typeof(UtcDateTimeConverter))]
    public DateTime Timestamp { get; set; }
}
