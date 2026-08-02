using System.Text.Json.Serialization;

namespace Fip.Strive.Ingestion.Application.Vendors.Polar.Flow.SleepScore
{
    public class PolarFlowSleepScore
    {
        [JsonPropertyName("night")]
        public DateTimeOffset Night { get; set; }

        [JsonPropertyName("sleepScoreResult")]
        public Dictionary<string, double> SleepScoreResult { get; set; } = new();

        [JsonPropertyName("sleepScoreBaselines")]
        public Dictionary<string, double> SleepScoreBaselines { get; set; } = new();
    }
}
