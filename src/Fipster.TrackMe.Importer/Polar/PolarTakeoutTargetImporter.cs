using Fipster.TrackMe.Domain;
using Fipster.TrackMe.Domain.Tools;
using Fipster.TrackMe.Importer.Modules.Abstractions;

namespace Fipster.TrackMe.Importer.Polar
{
    public class PolarTakeoutTargetImporter : IFileClassifier
    {
        public string Type => "polar_takeout_target";
        public DataSources Source => DataSources.PolarTakeoutTarget;

        public ImportClassification? Classify(string filepath)
        {
            var peeker = new FilePeeker(filepath);

            var result = peeker.ReadChars(128);
            if (!result.Contains("\"done\""))
                return null;

            var date = peeker.ReadTokens("startTime");
            if (string.IsNullOrWhiteSpace(date))
                return null;

            return new ImportClassification
            {
                Filepath = filepath,
                Source = Source,
                Filetype = Type,
                Datetime = DateTime.Parse(date),
                Datetype = DateRanges.Time,
                Skip = true,
            };
        }
    }
}
