namespace TheFipster.ActivityAggregator.Polar.Domain
{
    using System;
    using System.Collections.Generic;

    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Globalization;

    public partial class PolarTakeoutSleepResult
    {
        [JsonPropertyName("night")]
        public DateTimeOffset Night { get; set; }

        [JsonPropertyName("evaluation")]
        public Evaluation Evaluation { get; set; }

        [JsonPropertyName("sleepResult")]
        public SleepResult SleepResult { get; set; }
    }

    public partial class Evaluation
    {
        [JsonPropertyName("sleepType")]
        public string SleepType { get; set; }

        [JsonPropertyName("sleepSpan")]
        public string SleepSpan { get; set; }

        [JsonPropertyName("asleepDuration")]
        public string AsleepDuration { get; set; }

        [JsonPropertyName("age")]
        public double Age { get; set; }

        [JsonPropertyName("analysis")]
        public Analysis Analysis { get; set; }

        [JsonPropertyName("interruptions")]
        public Interruptions Interruptions { get; set; }
    }

    public partial class Analysis
    {
        [JsonPropertyName("efficiencyPercent")]
        public double EfficiencyPercent { get; set; }

        [JsonPropertyName("continuityIndex")]
        public double ContinuityIndex { get; set; }

        [JsonPropertyName("continuityClass")]
        public long ContinuityClass { get; set; }

        [JsonPropertyName("feedback")]
        public long Feedback { get; set; }
    }

    public partial class Interruptions
    {
        [JsonPropertyName("totalDuration")]
        public string TotalDuration { get; set; }

        [JsonPropertyName("totalCount")]
        public long TotalCount { get; set; }

        [JsonPropertyName("shortDuration")]
        public string ShortDuration { get; set; }

        [JsonPropertyName("shortCount")]
        public long ShortCount { get; set; }

        [JsonPropertyName("longDuration")]
        public string LongDuration { get; set; }

        [JsonPropertyName("longCount")]
        public long LongCount { get; set; }
    }

    public partial class SleepResult
    {
        [JsonPropertyName("hypnogram")]
        public Hypnogram Hypnogram { get; set; }
    }

    public partial class Hypnogram
    {
        [JsonPropertyName("sleepStart")]
        public DateTimeOffset SleepStart { get; set; }

        [JsonPropertyName("sleepEnd")]
        public DateTimeOffset SleepEnd { get; set; }

        [JsonPropertyName("alarmSnoozeTimes")]
        public List<object> AlarmSnoozeTimes { get; set; }

        [JsonPropertyName("sleepStateChanges")]
        public List<SleepStateChange> SleepStateChanges { get; set; }

        [JsonPropertyName("sleepStartOffset")]
        public long SleepStartOffset { get; set; }

        [JsonPropertyName("sleepEndOffset")]
        public long SleepEndOffset { get; set; }

        [JsonPropertyName("rating")]
        public string Rating { get; set; }

        [JsonPropertyName("birthday")]
        public DateTimeOffset Birthday { get; set; }

        [JsonPropertyName("sleepGoal")]
        public string SleepGoal { get; set; }

        [JsonPropertyName("deviceId")]
        public string DeviceId { get; set; }

        [JsonPropertyName("batteryRanOut")]
        public bool BatteryRanOut { get; set; }
    }

    public partial class SleepStateChange
    {
        [JsonPropertyName("offsetFromStart")]
        public string OffsetFromStart { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; }
    }
}
