using System.Security.Cryptography;
using System.Text.Json;
using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Models;
using Microsoft.Extensions.Configuration;

namespace TheFipster.ActivityAggregator.Importer.Modules
{
    public class DataIndexer
    {
        private readonly string filepath;

        public DataIndexer(string filepath)
        {
            this.filepath = filepath;
            if (!File.Exists(filepath))
                using (StreamWriter newFile = new StreamWriter(filepath))
                    newFile.WriteLine("[]");

            var json = File.ReadAllText(filepath);
            Index = JsonSerializer.Deserialize<List<DataIndex>>(json)!;

            if (Index == null)
                throw new ArgumentException($"File {filepath} couldn't be parsed.");
        }

        public DataIndexer(IConfiguration config)
            : this(config["IndexFile"] ?? throw new ArgumentException("IndexFile is not set.")) { }

        public List<DataIndex> Index { get; private set; }

        public void Add(string filepath, FileExtraction param)
        {
            if (!File.Exists(filepath))
                throw new ArgumentException($"File {filepath} doesn't exist.");

            var hash = calculateMd5(filepath);
            var fileIndex = new DataIndex
            {
                Date = param.Timestamp,
                Range = param.Range,
                Filepath = filepath,
                Source = param.Source,
                SourceFile = param.SourceFile,
                Type = FileTypes.Params,
                Md5Hash = hash,
            };

            Index = Index.Where(x => x.Md5Hash != hash).ToList();
            Index.Add(fileIndex);
        }

        public void Add(string filepath, GenericSeries series)
        {
            if (!File.Exists(filepath))
                throw new ArgumentException($"File {filepath} doesn't exist.");

            var hash = calculateMd5(filepath);
            var fileIndex = new DataIndex
            {
                Date = series.Date,
                Range = series.Range,
                Filepath = filepath,
                Source = series.Source,
                Type = FileTypes.Series,
                Md5Hash = hash,
            };

            Index = Index.Where(x => x.Md5Hash != hash).ToList();
            Index.Add(fileIndex);
        }

        public void Write()
        {
            var json = JsonSerializer.Serialize(Index);
            File.WriteAllText(filepath, json);
        }

        private string calculateMd5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter
                        .ToString(hash)
                        .Replace("-", string.Empty)
                        .ToLowerInvariant();
                }
            }
        }
    }
}
