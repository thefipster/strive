namespace TheFipster.ActivityAggregator.Polar.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class PolarTakeoutSleepScore
    {
        [JsonPropertyName("night")]
        public DateTimeOffset Night { get; set; }

        [JsonPropertyName("sleepScoreResult")]
        public Dictionary<string, double> SleepScoreResult { get; set; } = new();

        [JsonPropertyName("sleepScoreBaselines")]
        public Dictionary<string, double> SleepScoreBaselines { get; set; } = new();
    }
}
