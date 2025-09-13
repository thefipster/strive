using Fipster.TrackMe.Domain;
using Fipster.TrackMe.Domain.Tools;
using Fipster.TrackMe.Importer.Modules.Abstractions;

namespace Fipster.TrackMe.Importer.Gpsies
{
    public class GpsiesCsvImporter : IFileClassifier
    {
        public string Type => "gpsies_csv";
        public DataSources Source => DataSources.GpsiesCsv;

        private List<string> Header = new() { "Latitude,Longitude,Elevation" };

        public ImportClassification? Classify(string filepath)
        {
            var peeker = new FilePeeker(filepath);
            var lines = peeker.ReadLines(2);

            if (lines == null || lines.Count() != 2)
                return null;

            if (Header.All(x => x != lines.First()))
                return null;

            return new ImportClassification
            {
                Filepath = filepath,
                Source = Source,
                Filetype = Type,
                Datetype = DateRanges.Day,
            };
        }
    }
}
