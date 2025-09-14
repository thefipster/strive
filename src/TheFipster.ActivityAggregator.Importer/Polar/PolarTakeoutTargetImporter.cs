using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Importer.Modules.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Polar
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
