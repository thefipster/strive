namespace Fipster.TrackMe.Polar.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public partial class PolarTakeoutCalendarItems
    {
        [JsonPropertyName("exportVersion")]
        public string ExportVersion { get; set; }

        [JsonPropertyName("perceivedRecovery")]
        public List<object> PerceivedRecovery { get; set; }

        [JsonPropertyName("physicalInformations")]
        public List<PhysicalInformation> PhysicalInformations { get; set; }

        [JsonPropertyName("notes")]
        public List<Note> Notes { get; set; }

        [JsonPropertyName("feelings")]
        public List<Feeling> Feelings { get; set; }

        [JsonPropertyName("weights")]
        public List<Weight> Weights { get; set; }

        [JsonPropertyName("feedbacks")]
        public List<object> Feedbacks { get; set; }
    }

    public partial class Feeling
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }

        [JsonPropertyName("date")]
        public DateTimeOffset Date { get; set; }
    }

    public partial class Note
    {
        [JsonPropertyName("note")]
        public string NoteNote { get; set; }

        [JsonPropertyName("date")]
        public DateTimeOffset Date { get; set; }
    }

    public class PhysicalInformation
    {
        [JsonPropertyName("dateTime")]
        public DateTimeOffset DateTime { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("trainingBackground")]
        public string? TrainingBackground { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("vo2Max")]
        public long? Vo2Max { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("maximumHeartRate")]
        public long? MaximumHeartRate { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("restingHeartRate")]
        public long? RestingHeartRate { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("aerobicThreshold")]
        public long? AerobicThreshold { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("anaerobicThreshold")]
        public long? AnaerobicThreshold { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("typicalDay")]
        public string? TypicalDay { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("sleepGoal")]
        public string? SleepGoal { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("functionalThresholdPower")]
        public long? FunctionalThresholdPower { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("sex")]
        public string? Sex { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("birthday")]
        public DateTime? Birthday { get; set; }
    }

    public partial class Weight
    {
        [JsonPropertyName("weight")]
        public double Value { get; set; }

        [JsonPropertyName("date")]
        public DateTime Date { get; set; }
    }
}
