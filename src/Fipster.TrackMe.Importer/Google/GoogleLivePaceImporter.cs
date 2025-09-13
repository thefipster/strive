using Fipster.TrackMe.Domain;
using Fipster.TrackMe.Domain.Tools;
using Fipster.TrackMe.Importer.Modules.Abstractions;

namespace Fipster.TrackMe.Importer.Modules.Google
{
    public class GoogleLivePaceImporter : IFileClassifier
    {
        public string Type => "google_live_pace";
        public DataSources Source => DataSources.FitbitTakeoutLivePace;

        private List<string> Header = new()
        {
            "timestamp,steps,distance millimeters,altitude gain millimeters",
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
