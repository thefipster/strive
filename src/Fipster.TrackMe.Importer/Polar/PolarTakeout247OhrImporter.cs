using Fipster.TrackMe.Domain;
using Fipster.TrackMe.Domain.Tools;
using Fipster.TrackMe.Importer.Modules.Abstractions;

namespace Fipster.TrackMe.Importer.Polar
{
    public class PolarTakeout247OhrImporter : IFileClassifier
    {
        public string Type => "polar_takeout_247ohr";
        public DataSources Source => DataSources.PolarTakeout247Ohr;

        public ImportClassification? Classify(string filepath)
        {
            var peeker = new FilePeeker(filepath);

            var result = peeker.ReadChars(256);
            if (!result.Contains("\"deviceDays\""))
                return null;

            var date = peeker.ReadTokens("date");
            if (string.IsNullOrWhiteSpace(date))
                return null;

            return new ImportClassification
            {
                Filepath = filepath,
                Source = Source,
                Filetype = Type,
                Datetime = DateTime.Parse(date),
                Datetype = DateRanges.Month,
            };
        }
    }
}
