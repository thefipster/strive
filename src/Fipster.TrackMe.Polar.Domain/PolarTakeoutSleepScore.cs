namespace Fipster.TrackMe.Polar.Domain
{
    using System;
    using System.Collections.Generic;

    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Globalization;

    public partial class PolarTakeoutSleepScore
    {
        [JsonPropertyName("night")]
        public DateTimeOffset Night { get; set; }

        [JsonPropertyName("sleepScoreResult")]
        public Dictionary<string, double> SleepScoreResult { get; set; }

        [JsonPropertyName("sleepScoreBaselines")]
        public Dictionary<string, double> SleepScoreBaselines { get; set; }
    }
}