namespace TheFipster.ActivityAggregator.Polar.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class PolarTakeoutSleepScore
    {
        [JsonPropertyName("night")]
        public DateTimeOffset Night { get; set; }

        [JsonPropertyName("sleepScoreResult")]
        public Dictionary<string, double> SleepScoreResult { get; set; }

        [JsonPropertyName("sleepScoreBaselines")]
        public Dictionary<string, double> SleepScoreBaselines { get; set; }
    }
}
