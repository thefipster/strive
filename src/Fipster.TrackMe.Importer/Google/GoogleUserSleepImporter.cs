using Fipster.TrackMe.Domain;
using Fipster.TrackMe.Domain.Tools;
using Fipster.TrackMe.Importer.Modules.Abstractions;

namespace Fipster.TrackMe.Importer.Modules.Google
{
    public class GoogleUserSleepImporter : IFileClassifier
    {
        public string Type => "google_user_sleep";
        public DataSources Source => DataSources.FitbitTakeoutUserSleep;

        private List<string> Header = new()
        {
            "sleep_id,sleep_type,minutes_in_sleep_period,minutes_after_wake_up,minutes_to_fall_asleep,minutes_asleep,minutes_awake,minutes_longest_awakening,minutes_to_persistent_sleep,start_utc_offset,sleep_start,end_utc_offset,sleep_end,data_source,sleep_created,sleep_last_updated",
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
