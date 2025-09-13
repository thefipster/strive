using System.Text.Json;
using Fipster.TrackMe.Domain.Extensions;
using Fipster.TrackMe.Domain.Models;
using Fipster.TrackMe.Importer.Modules;
using Fipster.TrackMe.Importer.Modules.Abstractions;

namespace Fipster.TrackMe.Cli.Components
{
    internal class Assimilator
    {
        private readonly string archivePath;
        private readonly string storagePath;
        private readonly List<IFileExtractor> extractors;
        private readonly DataIndexer storage;

        public Assimilator(string archive, string storage, List<IFileExtractor> extractors)
        {
            archivePath = archive;
            storagePath = storage;

            this.extractors = extractors;
            this.storage = new DataIndexer(Path.Combine(storagePath, "index.json"));
        }

        public void Run()
        {
            Console.WriteLine("Assimilating your life...");
            Console.WriteLine();

            var archive = new FileIndexer(Path.Combine(archivePath, "index.json"));

            foreach (var item in extractors)
            {
                Console.WriteLine(item.Source);

                var matches = archive.Index.Where(x => x.Source == item.Source);
                foreach (var match in matches)
                {
                    Console.Write("\t" + match.Filepath.PadRight(128));

                    try
                    {
                        var files = item.Extract(match);
                        saveFiles(files);
                    }
                    catch (ArgumentNullException ex)
                    {
                        Console.Write($" - error - " + ex.Message);
                    }
                    catch (ArgumentException ex)
                    {
                        Console.Write($" - error - " + ex.Message);
                    }
                    catch (NotSupportedException ex)
                    {
                        Console.Write($" - unsupported - " + ex.Message);
                    }

                    Console.WriteLine();
                }

                storage.Write();
            }
        }

        private void saveFiles(List<FileExtraction> files)
        {
            foreach (var item in files)
            {
                var guid = Guid.NewGuid().ToString();
                var filename =
                    $"{item.Timestamp.ToRangeString(item.Range)}-{item.Source}-{guid}.json";
                var path = Path.Combine(storagePath, item.Range.GetPath(item.Timestamp));
                var newFile = Path.Combine(path, filename);
                var json = JsonSerializer.Serialize(item);

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                if (!File.Exists(newFile))
                    File.WriteAllText(newFile, json);

                storage.Add(newFile, item);
            }

            storage.Write();
        }
    }
}
