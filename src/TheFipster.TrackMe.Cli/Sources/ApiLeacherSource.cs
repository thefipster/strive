using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Importer.Abstractions;
using TheFipster.ActivityAggregator.Importer.Modules.Abstractions;

namespace Fipster.TrackMe.Importer.Cli.Sources
{
    public class ApiLeacherSource : ISourceLoader
    {
        private readonly List<IFileClassifier> importers = new() { };

        public List<IFileClassifier> GetImporters() => importers;

        public Dictionary<string, List<ImportClassification>> Scan(string myConversion)
        {
            var files = new Dictionary<string, List<ImportClassification>>();

            var years = Directory.GetDirectories(myConversion);
            foreach (var year in years)
            {
                var yearDir = new DirectoryInfo(year);
                var months = Directory.GetDirectories(year);
                foreach (var month in months)
                {
                    var monthDir = new DirectoryInfo(month);
                    var monthDate = DateTime.Parse($"{monthDir.Name}-01");
                    var monthFiles = Directory.GetFiles(month);
                    foreach (var file in monthFiles)
                    {
                        var fileInfo = new FileInfo(file);
                        var classification = new ImportClassification
                        {
                            Filepath = file,
                            Datetime = monthDate,
                            Datetype = DateRanges.Month,
                        };

                        files.Add(file, new List<ImportClassification> { classification });
                    }

                    var days = Directory.GetDirectories(month);
                    foreach (var day in days)
                    {
                        var dayDir = new DirectoryInfo(day);
                        var dayDate = DateTime.Parse(dayDir.Name);
                        var dayFiles = Directory.GetFiles(day);
                        foreach (var file in dayFiles)
                        {
                            var classification = new ImportClassification
                            {
                                Filepath = file,
                                Datetime = dayDate,
                                Datetype = DateRanges.Day,
                            };
                            files.Add(file, new List<ImportClassification> { classification });
                        }
                    }
                }
            }

            return files;
        }
    }
}
