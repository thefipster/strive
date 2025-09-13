using Fipster.TrackMe.Domain;
using Fipster.TrackMe.Domain.Tools;
using Fipster.TrackMe.Importer.Modules.Abstractions;

namespace Fipster.TrackMe.Importer.Modules.Google
{
    public class GoogleDailyHeartrateVariabilityImporter : IFileClassifier
    {
        public string Type => "google_daily_heartrate_variablitity";
        public DataSources Source => DataSources.FitbitTakeoutDailyHeartrateVariability;

        private List<string> Header = new()
        {
            "timestamp,average heart rate variability milliseconds,non rem heart rate beats per minute,entropy,deep sleep root mean square of successive differences milliseconds",
        };

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
                Datetype = DateRanges.Month,
            };
        }
    }
}
