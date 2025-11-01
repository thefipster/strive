using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Fip.Strive.Core.Domain.Extensions;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Components;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;

namespace Fip.Strive.Core.Domain.Schemas.Ingestion.Models
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
            DataKind kind
        )
            : this()
        {
            Source = source;
            SourceFile = sourceFile;
            Timestamp = timestamp;
            Kind = kind;
        }

        public FileExtraction(
            DataSources source,
            string sourceFile,
            DateTime timestamp,
            DataKind kind,
            Dictionary<Parameters, string> attributes
        )
            : this(source, sourceFile, timestamp, kind)
        {
            Attributes = attributes;
        }

        public FileExtraction(
            DataSources source,
            string sourceFile,
            DateTime timestamp,
            DataKind kind,
            Dictionary<Parameters, List<string>> series
        )
            : this(source, sourceFile, timestamp, kind)
        {
            Series = series;
        }

        public FileExtraction(
            DataSources source,
            string sourceFile,
            DateTime timestamp,
            DataKind kind,
            Dictionary<Parameters, string> attributes,
            Dictionary<Parameters, List<string>> series
        )
            : this(source, sourceFile, timestamp, kind, attributes)
        {
            Series = series;
        }

        public DateTime Timestamp { get; set; }

        public DataKind Kind { get; set; }

        public DataSources Source { get; set; }

        public string SourceFile { get; set; }

        public string Hash { get; set; }

        public Dictionary<Parameters, string> Attributes { get; set; }

        public Dictionary<Parameters, List<string>> Series { get; set; }

        public List<UnifiedEvent> Events { get; set; } = new();

        public void AddSeries(Parameters parameter) => Series.Add(parameter, []);

        public void AddAttribute(Parameters parameter, string value) =>
            Attributes.Add(parameter, value);

        public static Dictionary<Parameters, string> EmptyAttributes => new();

        public static Dictionary<Parameters, List<string>> EmptySeries => new();

        public byte[] ToHash()
        {
            var metrics = string.Join(
                ",",
                Attributes.OrderBy(x => x.Key).Select(x => x.ToString())
            );

            var series = string.Join(
                ",",
                Series.OrderBy(x => x.Key).Select(x => $"{x.Key}:{string.Join(";", x.Value)}")
            );

            var events = string.Join(
                ",",
                Events.OrderBy(x => x.Timestamp).Select(x => x.ToString())
            );

            var combined = $"{metrics} | {series} | {events}";
            using var sha = SHA256.Create();
            return sha.ComputeHash(Encoding.UTF8.GetBytes(combined));
        }

        public string ToHashString()
        {
            var bytes = ToHash();
            Hash = Convert.ToHexString(bytes);
            return Hash;
        }

        public string Write(string rootDir)
        {
            Hash = ToHashString();
            var filename = $"{Timestamp.ToRangeString(Kind)}-{Source}-{Hash}.json";
            var datePath =
                Kind == DataKind.Day
                    ? Timestamp.ToString(DateHelper.DayFormat)
                    : Timestamp.ToString(DateHelper.FsMillisecondFormat);
            var path = Path.Combine(rootDir, datePath);
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

        public static FileExtraction FromFile(string filepath)
        {
            if (!File.Exists(filepath))
                throw new ArgumentException("File doesn't exist.", nameof(filepath));

            var json = File.ReadAllText(filepath);
            return FromJson(json);
        }
    }
}
