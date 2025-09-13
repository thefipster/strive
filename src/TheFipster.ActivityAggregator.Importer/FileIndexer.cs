using System.Security.Cryptography;
using System.Text.Json;
using TheFipster.ActivityAggregator.Domain.Models;
using TheFipster.ActivityAggregator.Importer.Modules.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Modules
{
    public class FileIndexer
    {
        private readonly string filepath;

        public FileIndexer(string filepath)
        {
            this.filepath = filepath;
            if (!File.Exists(filepath))
                using (StreamWriter newFile = new StreamWriter(filepath))
                    newFile.WriteLine("[]");

            var json = File.ReadAllText(filepath);
            Index = JsonSerializer.Deserialize<List<ArchiveIndex>>(json)!;

            if (Index == null)
                throw new ArgumentException($"File {filepath} couldn't be parsed.");
        }

        public List<ArchiveIndex> Index { get; private set; }

        public void Add(string filepath, string hash, ImportClassification classification)
        {
            if (!File.Exists(filepath))
                throw new ArgumentException($"File {filepath} doesn't exist.");

            var fileIndex = new ArchiveIndex
            {
                Date = classification.Datetime,
                Range = classification.Datetype,
                Filepath = filepath,
                Source = classification.Source,
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

        public static string CalculateMd5(string filename)
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
