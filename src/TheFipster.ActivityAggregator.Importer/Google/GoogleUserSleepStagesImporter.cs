using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Importer.Modules.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Modules.Google
{
    public class GoogleUserSleepStagesImporter : IFileClassifier
    {
        public string Type => "google_user_sleep_stages";
        public DataSources Source => DataSources.FitbitTakeoutUserSleepStages;

        private List<string> Header = new()
        {
            "sleep_id,sleep_stage_id,sleep_stage_type,start_utc_offset,sleep_stage_start,end_utc_offset,sleep_stage_end,data_source,sleep_stage_created,sleep_stage_last_updated",
        };

        public ImportClassification? Classify(string filepath)
        {
            var peeker = new FilePeeker(filepath);

            var header = peeker.ReadLines(1);
            if (header.Count() == 0 || Header.All(x => x != header.First()))
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
