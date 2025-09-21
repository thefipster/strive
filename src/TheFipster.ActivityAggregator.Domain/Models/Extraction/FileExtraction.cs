using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Extensions;

namespace TheFipster.ActivityAggregator.Domain.Models
{
    public class FileExtraction
    {
        public FileExtraction()
        {
            Attributes = new();
            Series = new();
            SourceFile = string.Empty;
            Hash = string.Empty;
        }

        public FileExtraction(
            DataSources source,
            string sourceFile,
            DateTime timestamp,
            DateRanges range
        )
            : this()
        {
            Source = source;
            SourceFile = sourceFile;
            Timestamp = timestamp;
            Range = range;
        }

        public FileExtraction(
            DataSources source,
            string sourceFile,
            DateTime timestamp,
            DateRanges range,
            Dictionary<Parameters, string> attributes
        )
            : this(source, sourceFile, timestamp, range)
        {
            Attributes = attributes;
        }

        public FileExtraction(
            DataSources source,
            string sourceFile,
            DateTime timestamp,
            DateRanges range,
            Dictionary<Parameters, List<string>> series
        )
            : this(source, sourceFile, timestamp, range)
        {
            Series = series;
        }

        public FileExtraction(
            DataSources source,
            string sourceFile,
            DateTime timestamp,
            DateRanges range,
            Dictionary<Parameters, string> attributes,
            Dictionary<Parameters, List<string>> series
        )
            : this(source, sourceFile, timestamp, range, attributes)
        {
            Series = series;
        }

        [JsonPropertyName("date")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("range")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DateRanges Range { get; set; }

        [JsonPropertyName("source")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DataSources Source { get; set; }

        [JsonPropertyName("sourceFile")]
        public string SourceFile { get; set; }

        [JsonPropertyName("hash")]
        public string Hash { get; set; }

        [JsonPropertyName("attributes")]
        public Dictionary<Parameters, string> Attributes { get; set; }

        [JsonPropertyName("series")]
        public Dictionary<Parameters, List<string>> Series { get; set; }

        [JsonPropertyName("events")]
        public List<UnifiedEvent> Events { get; set; } = new();

        public static Dictionary<Parameters, string> EmptyAttributes => new();

        public static Dictionary<Parameters, List<string>> EmptySeries => new();

        public static FileExtraction FromFile(string filepath)
        {
            if (!File.Exists(filepath))
                throw new ArgumentException("File doesn't exist.", nameof(filepath));

            var json = File.ReadAllText(filepath);
            return FromJson(json);
        }

        public string GetValueHash()
        {
            var metrics = string.Join(
                ",",
                Attributes.OrderBy(kvp => kvp.Key).Select(kvp => $"{kvp.Key}:{kvp.Value}")
            );

            var series = string.Join(
                ",",
                Series
                    .OrderBy(kvp => kvp.Key)
                    .Select(kvp => $"{kvp.Key}:{string.Join(";", kvp.Value)}")
            );

            var combined = metrics + "|" + series;
            using var sha = SHA256.Create();
            var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(combined));

            Hash = Convert.ToHexString(hashBytes);
            return Hash;
        }

        public string Write(string rootDir)
        {
            Hash = GetValueHash();
            var filename = $"{Timestamp.ToRangeString(Range)}-{Source}-{Hash}.json";
            var path = Path.Combine(rootDir, Range.GetPath(Timestamp));
            var newFile = Path.Combine(path, filename);
            var json = JsonSerializer.Serialize(this);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            if (!File.Exists(newFile))
                File.WriteAllText(newFile, json);

            return newFile;
        }

        public static FileExtraction FromJson(string json)
        {
            var extract = JsonSerializer.Deserialize<FileExtraction>(json);
            if (extract == null)
                throw new ArgumentException("Invalid json", nameof(json));

            return extract;
        }
    }
}
