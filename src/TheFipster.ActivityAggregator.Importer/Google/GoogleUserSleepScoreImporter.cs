using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Importer.Modules.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Modules.Google
{
    public class GoogleUserSleepScoreImporter : IFileClassifier
    {
        public string Type => "google_user_sleep_score";
        public DataSources Source => DataSources.FitbitTakeoutUserSleepScore;

        private List<string> Header = new()
        {
            "sleep_id,sleep_score_id,data_source,score_utc_offset,score_time,overall_score,duration_score,composition_score,revitalization_score,sleep_time_minutes,deep_sleep_minutes,rem_sleep_percent,resting_heart_rate,sleep_goal_minutes,waso_count_long_wakes,waso_count_all_wake_time,restlessness_normalized,hr_below_resting_hr,sleep_score_created,sleep_score_last_updated",
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
