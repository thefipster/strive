using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Importer.Modules.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Modules.Google
{
    public class GoogleDemographicVo2MaxImporter : IFileClassifier
    {
        public string Type => "google_demographic_vo2max";
        public DataSources Source => DataSources.FitbitTakeoutDemographicVo2Max;

        private List<string> Header = new() { "timestamp,demographic vo2max" };

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
