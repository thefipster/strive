using Fipster.TrackMe.Domain;
using Fipster.TrackMe.Domain.Tools;
using Fipster.TrackMe.Importer.Modules.Abstractions;

namespace Fipster.TrackMe.Importer.Modules.Google
{
    public class GoogleHeartrateNotificationImporter : IFileClassifier
    {
        public string Type => "google_heartrate_notification";
        public DataSources Source => DataSources.FitbitTakeoutHeartrateNotification;

        private List<string> Header = new()
        {
            "timestamp,heart rate notification type,heart rate threshold beats per minute,heart rate trigger value beats per minute",
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
