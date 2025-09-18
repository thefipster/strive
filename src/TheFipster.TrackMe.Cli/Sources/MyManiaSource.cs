using TheFipster.ActivityAggregator.Importer.Abstractions;
using TheFipster.ActivityAggregator.Importer.Generic;
using TheFipster.ActivityAggregator.Importer.Gpsies;
using TheFipster.ActivityAggregator.Importer.Modules.Abstractions;
using TheFipster.ActivityAggregator.Importer.Polar;
using TheFipster.ActivityAggregator.Importer.RunGps;

namespace Fipster.TrackMe.Importer.Cli.Sources
{
    public class MyManiaSource : ISourceLoader
    {
        private readonly List<IFileClassifier> importers = new()
        {
            new PolarProTrainerHrmImporter(),
            new RunGpsCsvImporter(),
            new RunGpsCsv2Importer(),
            new GpsiesCsvImporter(),
            new KmlImporter(),
            new GpxImporter(),
        };

        public List<IFileClassifier> GetImporters() => importers;

        public Dictionary<string, List<ImportClassification>> Scan(string myCollection)
        {
            var result = new Dictionary<string, List<ImportClassification>>();
            var years = Directory.GetDirectories(myCollection);
            foreach (var year in years)
            {
                var trainings = Directory.GetDirectories(year);
                foreach (var training in trainings)
                {
                    var trainingDir = new DirectoryInfo(training);
                    DateTime date = GetDateFromDir(trainingDir);
                    var files = Directory.GetFiles(training, "*.*", SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        var fileInfo = new FileInfo(file);
                        if (
                            fileInfo.Name.StartsWith(".")
                            || fileInfo.Extension == ".zip"
                            || fileInfo.Extension == ".html"
                            || fileInfo.Extension == ".htm"
                            || fileInfo.Extension == ".txt"
                            || fileInfo.Extension == ".png"
                            || fileInfo.Extension == ".crs"
                            || fileInfo.Extension == ".kmz"
                            || fileInfo.Extension == ".ovl"
                            || fileInfo.Extension == ".tcx"
                            || fileInfo.Extension == ".trk"
                            || fileInfo.Extension == ".xls"
                            || fileInfo.Extension == ".xml"
                        )
                            continue;

                        var classification = Classify(file, date);
                        result.Add(file, classification);
                    }
                }
            }

            return result;
        }

        private List<ImportClassification> Classify(string file, DateTime date)
        {
            var results = new List<ImportClassification>();

            foreach (var importer in importers)
            {
                var result = importer.Classify(file);
                if (result == null)
                    continue;

                if (result.Datetime == DateTime.MinValue)
                    result.Datetime = date;

                results.Add(result);
            }

            return results;
        }

        private static DateTime GetDateFromDir(DirectoryInfo trainingDir)
        {
            var datetimeParts = trainingDir.Name.Split(" - ");
            var dateString = datetimeParts[0].Replace(".", "-");
            var hours = int.Parse(datetimeParts[1].Substring(0, 2));
            var minutes = int.Parse(datetimeParts[1].Substring(2, 2));
            var date = DateTime.Parse(dateString).AddHours(hours).AddMinutes(minutes);
            return date;
        }
    }
}
