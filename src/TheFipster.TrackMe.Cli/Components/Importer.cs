using Fipster.TrackMe.Importer.Cli.Models;
using TheFipster.ActivityAggregator.Importer.Modules;
using TheFipster.ActivityAggregator.Importer.Modules.Abstractions;

namespace Fipster.TrackMe.Cli.Components
{
    internal class Importer(string archive, Dictionary<ISourceLoader, List<string>> sources)
    {
        public void Run()
        {
            Console.WriteLine("Importing your life...");
            Console.WriteLine();

            var indexer = new FileIndexer(Path.Combine(archive, "index.json"));
            foreach (var source in sources)
            {
                foreach (var directory in source.Value)
                {
                    var reader = source.Key;
                    PrintImporter(directory, reader);
                    var files = reader.Scan(directory);
                    var result = CheckResults(files);
                    PrintResult(result);

                    var counter = 0;
                    foreach (var file in files.Where(x => x.Value.Count == 1))
                        counter += HandleFile(archive, indexer, file);

                    Console.Write($"   {counter, 6} files copied");

                    if (!result.Valid)
                    {
                        Console.WriteLine();
                        Console.WriteLine();

                        foreach (var file in files.Where(x => x.Value.Count == 0))
                            Console.WriteLine(file.Key);

                        Console.WriteLine();
                    }

                    Console.WriteLine();
                }

                indexer.Write();
            }
        }

        private static int HandleFile(
            string archive,
            FileIndexer indexer,
            KeyValuePair<string, List<ImportClassification>> file
        )
        {
            var counter = 0;
            var oldFile = file.Key;
            var fileInfo = new FileInfo(oldFile);
            var hash = FileIndexer.CalculateMd5(oldFile);
            var classification = file.Value.First();
            var path = Path.Combine(new[] { archive }.Concat(classification.DatePieces).ToArray());
            var filename =
                $"{hash}-{classification.Source}-{classification.Filetype}{fileInfo.Extension}";
            var newFile = Path.Combine(path, filename);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            if (!File.Exists(newFile))
            {
                File.Copy(oldFile, newFile);
                counter++;
            }

            indexer.Add(newFile, hash, classification);
            return counter;
        }

        private static void PrintImporter(string directory, ISourceLoader reader)
        {
            var dirInfo = new DirectoryInfo(directory);
            Console.Write(reader.GetType().Name.PadRight(32));
            Console.Write(dirInfo.Name.PadRight(32));
        }

        private static void PrintResult(ScanResult result)
        {
            Console.Write(result.Total.ToString().PadLeft(8));
            Console.Write(result.Percentage.ToString().PadLeft(6) + "%");
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(result.Misses > 0 ? " miss" : "");
            Console.Write(result.Doubles > 0 ? " doubles" : "");
            Console.ForegroundColor = color;
        }

        private ScanResult CheckResults(Dictionary<string, List<ImportClassification>> scan)
        {
            var result = new ScanResult();
            result.Total = scan.Count;
            result.Matches = scan.Count(x => x.Value.Count == 1);
            result.Misses = scan.Count(x => x.Value.Count == 0);
            result.Doubles = scan.Count(x => x.Value.Count > 1);

            return result;
        }
    }
}
