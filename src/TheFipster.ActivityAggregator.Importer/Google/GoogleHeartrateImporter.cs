using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Importer.Modules.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Modules.Google
{
    public class GoogleHeartrateImporter : IFileClassifier
    {
        public string Type => "google_heartrate";
        public DataSources Source => DataSources.FitbitTakeoutHeartrate;

        private List<string> Header = new() { "timestamp,beats per minute" };

        public ImportClassification? Classify(string filepath)
        {
            var peeker = new FilePeeker(filepath);

            var header = peeker.ReadLines(2);
            if (header.Count() != 2 || Header.All(x => x != header.First()))
                return null;

            var cells = header.Last().Split(",");
            var date = DateTime.Parse(cells[0]);

            return new ImportClassification
            {
                Filepath = filepath,
                Source = Source,
                Filetype = Type,
                Datetime = date,
                Datetype = filepath.Contains("daily") ? DateRanges.AllTime : DateRanges.Day,
            };
        }
    }
}
