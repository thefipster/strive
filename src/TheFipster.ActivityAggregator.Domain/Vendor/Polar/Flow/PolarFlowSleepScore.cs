using System.Text.Json.Serialization;

namespace TheFipster.ActivityAggregator.Domain.Vendor.Polar.Flow
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
