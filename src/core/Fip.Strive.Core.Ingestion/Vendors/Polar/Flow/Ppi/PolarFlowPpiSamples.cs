using System.Text.Json.Serialization;

namespace Fip.Strive.Core.Ingestion.Vendors.Polar.Flow.Ppi
{
    public class PolarFlowPpiSamples
    {
        [JsonPropertyName("date")]
        public DateTimeOffset Date { get; set; }

        [JsonPropertyName("devicePpiSamplesList")]
        public List<DevicePpiSamplesList> DevicePpiSamplesList { get; set; } = [];
    }

    public class DevicePpiSamplesList
    {
        [JsonPropertyName("deviceId")]
        public string? DeviceId { get; set; }

        [JsonPropertyName("ppiSamples")]
        public List<PpiSample> PpiSamples { get; set; } = [];
    }

    public class PpiSample
    {
        [JsonPropertyName("sampleDateTime")]
        public DateTimeOffset SampleDateTime { get; set; }

        [JsonPropertyName("pulseLength")]
        public int PulseLength { get; set; }
    }
}
