using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheFipster.ActivityAggregator.Domain.Models
{
    public class FileExtraction
    {
        public FileExtraction()
        {
            Attributes = new Dictionary<Parameters, string>();
            Series = new Dictionary<Parameters, IEnumerable<string>>();
            SourceFile = string.Empty;
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
            Dictionary<Parameters, string> attributes,
            Dictionary<Parameters, IEnumerable<string>> series
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

        [JsonPropertyName("attributes")]
        public Dictionary<Parameters, string> Attributes { get; set; }

        [JsonPropertyName("series")]
        public Dictionary<Parameters, IEnumerable<string>> Series { get; set; }

        public static Dictionary<Parameters, string> EmptyAttributes => new();

        public static Dictionary<Parameters, IEnumerable<string>> EmptySeries => new();

        public static FileExtraction FromFile(string filepath)
        {
            if (!File.Exists(filepath))
                throw new ArgumentException("File doesn't exist.", nameof(filepath));

            var json = File.ReadAllText(filepath);
            return FromJson(json);
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
