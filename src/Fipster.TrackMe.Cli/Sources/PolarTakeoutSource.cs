using System.Collections.Concurrent;
using Fipster.TrackMe.Importer.Modules.Abstractions;
using Fipster.TrackMe.Importer.Polar;

namespace Fipster.TrackMe.Importer.Cli.Sources
{
    public class PolarTakeoutSource : ISourceLoader
    {
        private readonly List<IFileClassifier> importers = new()
        {
            new PolarTakeout247OhrImporter(),
            new PolarTakeoutActivityImporter(),
            new PolarTakeoutPpiImporter(),
            new PolarTakeoutGenericPeriodImporter(),
            new PolarTakeoutTrainingImporter(),
            new PolarTakeoutTargetImporter(),
            new PolarTakeoutSleepResultImporter(),
            new PolarTakeoutSleepScoreImporter(),
            new PolarTakeoutCalendarItemsImporter(),
        };

        public List<IFileClassifier> GetImporters() => importers;

        public Dictionary<string, List<ImportClassification>> Scan(string importPath)
        {
            var classifications = new ConcurrentDictionary<string, List<ImportClassification>>();
            var files = Directory.GetFiles(importPath);

            Parallel.ForEach(
                files,
                file =>
                {
                    var classification = Classify(file);
                    while (!classifications.TryAdd(file, classification))
                        Thread.Sleep(100);
                }
            );

            return new Dictionary<string, List<ImportClassification>>(
                classifications,
                classifications.Comparer
            );
        }

        private List<ImportClassification> Classify(string file)
        {
            var results = new List<ImportClassification>();

            foreach (var importer in importers)
            {
                var result = importer.Classify(file);
                if (result == null)
                    continue;

                results.Add(result);
            }

            return results;
        }
    }
}
