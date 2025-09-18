using TheFipster.ActivityAggregator.Importer.Abstractions;
using TheFipster.ActivityAggregator.Importer.Modules.Abstractions;
using TheFipster.ActivityAggregator.Importer.Polar;

namespace Fipster.TrackMe.Importer.Cli.Sources
{
    public class PolarProTrainerSource : ISourceLoader
    {
        private readonly List<IFileClassifier> importers = new()
        {
            new PolarProTrainerHrmImporter(),
            new PolarProTrainerPddImporter(),
        };

        public List<IFileClassifier> GetImporters() => importers;

        public Dictionary<string, List<ImportClassification>> Scan(string importPath)
        {
            var result = new Dictionary<string, List<ImportClassification>>();
            var dirs = Directory.GetDirectories(importPath);

            foreach (var dir in dirs)
            {
                var files = Directory.GetFiles(dir);

                foreach (var file in files)
                {
                    var classification = Classify(file);
                    result.Add(file, classification);
                }
            }

            return result;
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
