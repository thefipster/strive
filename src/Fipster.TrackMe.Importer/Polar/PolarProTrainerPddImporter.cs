using Fipster.TrackMe.Domain;
using Fipster.TrackMe.Domain.Tools;
using Fipster.TrackMe.Importer.Modules.Abstractions;

namespace Fipster.TrackMe.Importer.Polar
{
    public class PolarProTrainerPddImporter : IFileClassifier
    {
        public string Type => "polar_protrainer_pdd";
        public DataSources Source => DataSources.PolarProTrainerPdd;

        public ImportClassification? Classify(string filepath)
        {
            var peeker = new FilePeeker(filepath);

            var result = peeker.ReadChars(128);
            if (result.Length < 9 || result.Substring(0, 9) != "[DayInfo]")
                return null;

            var lines = result.Split('\n');
            var line = lines[2];

            var kvp = line.Split("\t");
            var date = kvp[0];

            var year = date.Substring(0, 4);
            var month = date.Substring(4, 2);
            var day = date.Substring(6, 2);

            var datetime = DateTime.Parse($"{year}-{month}-{day}");

            return new ImportClassification
            {
                Filepath = filepath,
                Source = Source,
                Filetype = Type,
                Datetime = datetime,
                Datetype = DateRanges.Day,
            };
        }
    }
}
