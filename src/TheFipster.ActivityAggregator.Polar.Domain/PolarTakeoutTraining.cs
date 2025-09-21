namespace TheFipster.ActivityAggregator.Polar.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class PolarTakeoutTraining
    {
        [JsonPropertyName("exportVersion")]
        public string ExportVersion { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("deviceId")]
        public string DeviceId { get; set; }

        [JsonPropertyName("feeling")]
        public string Feeling { get; set; }

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("startTime")]
        public DateTimeOffset StartTime { get; set; }

        [JsonPropertyName("stopTime")]
        public DateTimeOffset StopTime { get; set; }

        [JsonPropertyName("timeZoneOffset")]
        public long TimeZoneOffset { get; set; }

        [JsonPropertyName("distance")]
        public double Distance { get; set; }

        [JsonPropertyName("duration")]
        public string Duration { get; set; }

        [JsonPropertyName("maximumHeartRate")]
        public long MaximumHeartRate { get; set; }

        [JsonPropertyName("averageHeartRate")]
        public long AverageHeartRate { get; set; }

        [JsonPropertyName("kiloCalories")]
        public long KiloCalories { get; set; }

        [JsonPropertyName("physicalInformationSnapshot")]
        public PhysicalInformationSnapshot PhysicalInformationSnapshot { get; set; }

        [JsonPropertyName("exercises")]
        public List<Exercise> Exercises { get; set; }

        [JsonPropertyName("periodData")]
        public PeriodData PeriodData { get; set; }

        [JsonPropertyName("loadInformation")]
        public LoadInformation LoadInformation { get; set; }
    }

    public class Exercise
    {
        [JsonPropertyName("startTime")]
        public DateTimeOffset StartTime { get; set; }

        [JsonPropertyName("stopTime")]
        public DateTimeOffset StopTime { get; set; }

        [JsonPropertyName("timezoneOffset")]
        public long TimezoneOffset { get; set; }

        [JsonPropertyName("duration")]
        public string Duration { get; set; }

        [JsonPropertyName("distance")]
        public double Distance { get; set; }

        [JsonPropertyName("sport")]
        public string Sport { get; set; }

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("ascent")]
        public double Ascent { get; set; }

        [JsonPropertyName("descent")]
        public double Descent { get; set; }

        [JsonPropertyName("kiloCalories")]
        public long KiloCalories { get; set; }

        [JsonPropertyName("altitude")]
        public ExerciseAltitude Altitude { get; set; }

        [JsonPropertyName("cadence")]
        public Dictionary<string, double> Cadence { get; set; }

        [JsonPropertyName("heartRate")]
        public ExerciseAltitude HeartRate { get; set; }

        [JsonPropertyName("power")]
        public Dictionary<string, double> Power { get; set; }

        [JsonPropertyName("speed")]
        public Dictionary<string, double> Speed { get; set; }

        [JsonPropertyName("zones")]
        public Zones Zones { get; set; }

        [JsonPropertyName("autoLaps")]
        public List<AutoLap> AutoLaps { get; set; }

        [JsonPropertyName("samples")]
        public Samples Samples { get; set; }

        [JsonPropertyName("loadInformation")]
        public LoadInformation LoadInformation { get; set; }
    }

    public class ExerciseAltitude
    {
        [JsonPropertyName("min")]
        public double Min { get; set; }

        [JsonPropertyName("avg")]
        public double Avg { get; set; }

        [JsonPropertyName("max")]
        public double Max { get; set; }
    }

    public class AutoLap
    {
        [JsonPropertyName("lapNumber")]
        public long LapNumber { get; set; }

        [JsonPropertyName("duration")]
        public string Duration { get; set; }

        [JsonPropertyName("splitTime")]
        public string SplitTime { get; set; }

        [JsonPropertyName("distance")]
        public double Distance { get; set; }

        [JsonPropertyName("heartRate")]
        public ExerciseAltitude HeartRate { get; set; }

        [JsonPropertyName("speed")]
        public Dictionary<string, double> Speed { get; set; }

        [JsonPropertyName("cadence")]
        public Dictionary<string, double> Cadence { get; set; }

        [JsonPropertyName("ascent")]
        public double Ascent { get; set; }

        [JsonPropertyName("descent")]
        public double Descent { get; set; }

        [JsonPropertyName("power")]
        public Dictionary<string, double> Power { get; set; }
    }

    public class LoadInformation
    {
        [JsonPropertyName("cardioLoad")]
        public double CardioLoad { get; set; }

        [JsonPropertyName("muscleLoad")]
        public double MuscleLoad { get; set; }

        [JsonPropertyName("cardioLoadInterpretation")]
        public string CardioLoadInterpretation { get; set; }

        [JsonPropertyName("muscleLoadInterpretation")]
        public string MuscleLoadInterpretation { get; set; }

        [JsonPropertyName("calculationTime")]
        public DateTimeOffset CalculationTime { get; set; }

        [JsonPropertyName("sessionRpe")]
        public string SessionRpe { get; set; }

        [JsonPropertyName("perceivedLoad")]
        public double PerceivedLoad { get; set; }

        [JsonPropertyName("perceivedLoadInterpretation")]
        public string PerceivedLoadInterpretation { get; set; }
    }

    public class Samples
    {
        [JsonPropertyName("rr")]
        public List<RrSample> Rr { get; set; }

        [JsonPropertyName("altitude")]
        public List<DoubleSample> Altitude { get; set; }

        [JsonPropertyName("heartRate")]
        public List<DoubleSample> HeartRate { get; set; }

        [JsonPropertyName("speed")]
        public List<DoubleSample> Speed { get; set; }

        [JsonPropertyName("cadence")]
        public List<DoubleSample> Cadence { get; set; }

        [JsonPropertyName("distance")]
        public List<DoubleSample> Distance { get; set; }

        [JsonPropertyName("temperature")]
        public List<DoubleSample> Temperature { get; set; }

        [JsonPropertyName("leftPedalCrankBasedPower")]
        public List<IntSample> LeftPedalCrankBasedPower { get; set; }

        [JsonPropertyName("recordedRoute")]
        public List<GpsSample> RecordedRoute { get; set; }
    }

    public class DoubleSample
    {
        [JsonPropertyName("dateTime")]
        public DateTimeOffset DateTime { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("value")]
        public double? Value { get; set; }
    }

    public class IntSample
    {
        [JsonPropertyName("dateTime")]
        public DateTimeOffset DateTime { get; set; }

        [JsonPropertyName("currentPower")]
        public int CurrentPower { get; set; }
    }

    public class GpsSample
    {
        [JsonPropertyName("dateTime")]
        public DateTimeOffset DateTime { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("altitude")]
        public double Altitude { get; set; }
    }

    public class RrSample
    {
        [JsonPropertyName("duration")]
        public string Duration { get; set; }

        [JsonPropertyName("offline")]
        public bool Offline { get; set; }
    }

    public class Zones
    {
        [JsonPropertyName("speed")]
        public List<HeartRate> Speed { get; set; }

        [JsonPropertyName("power")]
        public List<HeartRate> Power { get; set; }

        [JsonPropertyName("heart_rate")]
        public List<HeartRate> HeartRate { get; set; }
    }

    public class HeartRate
    {
        [JsonPropertyName("lowerLimit")]
        public double LowerLimit { get; set; }

        [JsonPropertyName("higherLimit")]
        public double HigherLimit { get; set; }

        [JsonPropertyName("inZone")]
        public string InZone { get; set; }

        [JsonPropertyName("zoneIndex")]
        public int ZoneIndex { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("distance")]
        public double? Distance { get; set; }
    }

    public class PeriodData
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("start")]
        public DateTimeOffset Start { get; set; }

        [JsonPropertyName("end")]
        public DateTimeOffset End { get; set; }

        [JsonPropertyName("attributes")]
        public List<PeriodDataAttribute> Attributes { get; set; }

        [JsonPropertyName("subPeriods")]
        public List<SubPeriod> SubPeriods { get; set; }
    }

    public class PeriodDataAttribute
    {
        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("textValue")]
        public string TextValue { get; set; }
    }

    public class SubPeriod
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("start")]
        public DateTimeOffset Start { get; set; }

        [JsonPropertyName("end")]
        public DateTimeOffset End { get; set; }

        [JsonPropertyName("attributes")]
        public List<SubPeriodAttribute> Attributes { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("subPeriods")]
        public List<SubPeriod> SubPeriods { get; set; }
    }

    public class SubPeriodAttribute
    {
        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("numericValue")]
        public double NumericValue { get; set; }
    }

    public class PhysicalInformationSnapshot
    {
        [JsonPropertyName("dateTime")]
        public DateTimeOffset DateTime { get; set; }

        [JsonPropertyName("sex")]
        public string Sex { get; set; }

        [JsonPropertyName("birthday")]
        public DateTimeOffset Birthday { get; set; }

        [JsonPropertyName("vo2Max")]
        public int Vo2Max { get; set; }

        [JsonPropertyName("maximumHeartRate")]
        public int MaximumHeartRate { get; set; }

        [JsonPropertyName("restingHeartRate")]
        public int RestingHeartRate { get; set; }

        [JsonPropertyName("aerobicThreshold")]
        public int AerobicThreshold { get; set; }

        [JsonPropertyName("anaerobicThreshold")]
        public int AnaerobicThreshold { get; set; }

        [JsonPropertyName("sleepGoal")]
        public string SleepGoal { get; set; }

        [JsonPropertyName("functionalThresholdPower")]
        public int FunctionalThresholdPower { get; set; }
    }
}
