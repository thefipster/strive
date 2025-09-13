using Fipster.TrackMe.Domain;
using Fipster.TrackMe.Domain.Tools;
using Fipster.TrackMe.Importer.Modules.Abstractions;

namespace Fipster.TrackMe.Importer.Modules.Google
{
    public class GoogleFloorsImporter : IFileClassifier
    {
        public string Type => "google_floors";
        public DataSources Source => DataSources.FitbitTakeoutFloors;

        private List<string> Header = new() { "timestamp,floors" };

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
