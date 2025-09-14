using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Importer.Modules.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Polar
{
    public class PolarTakeoutGenericPeriodImporter : IFileClassifier
    {
        public string Type => "polar_takeout_generic_period";
        public DataSources Source => DataSources.PolarTakeoutGenericPeriod;

        public ImportClassification? Classify(string filepath)
        {
            var peeker = new FilePeeker(filepath);

            var result = peeker.ReadChars(256);
            if (!result.Contains("\"meta\""))
                return null;

            var time = peeker.ReadTokens("startTimeSeconds");
            if (string.IsNullOrWhiteSpace(time))
                return null;

            var seconds = int.Parse(time);
            var date = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(seconds);

            return new ImportClassification
            {
                Filepath = filepath,
                Source = Source,
                Filetype = Type,
                Datetime = date,
                Datetype = DateRanges.Day,
            };
        }
    }
}
