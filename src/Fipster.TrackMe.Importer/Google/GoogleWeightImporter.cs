using Fipster.TrackMe.Domain;
using Fipster.TrackMe.Domain.Tools;
using Fipster.TrackMe.Importer.Modules.Abstractions;

namespace Fipster.TrackMe.Importer.Modules.Google
{
    public class GoogleWeightImporter : IFileClassifier
    {
        public string Type => "google_weight";
        public DataSources Source => DataSources.FitbitTakeoutWeight;

        private List<string> Header = new() { "timestamp,weight grams" };

        public ImportClassification? Classify(string filepath)
        {
            var peeker = new FilePeeker(filepath);

            var header = peeker.ReadLines(1);
            if (header.Count() != 1 || Header.All(x => x != header.First()))
                return null;

            return new ImportClassification
            {
                Filepath = filepath,
                Source = Source,
                Filetype = Type,
                Datetype = DateRanges.AllTime,
            };
        }
    }
}
