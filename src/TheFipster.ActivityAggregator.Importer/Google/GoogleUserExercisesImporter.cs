using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Importer.Modules.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Modules.Google
{
    public class GoogleUserExercisesImporter : IFileClassifier
    {
        public string Type => "google_user_exercises";
        public DataSources Source => DataSources.FitbitTakeoutUserExercises;

        private List<string> Header = new()
        {
            "exercise_id,exercise_start,exercise_end,utc_offset,exercise_created,exercise_last_updated,activity_name,log_type,pool_length,pool_length_unit,intervals,distance_units,tracker_total_calories,tracker_total_steps,tracker_total_distance_mm,tracker_total_altitude_mm,tracker_avg_heart_rate,tracker_peak_heart_rate,tracker_avg_pace_mm_per_second,tracker_avg_speed_mm_per_second,tracker_peak_speed_mm_per_second,tracker_auto_stride_run_mm,tracker_auto_stride_walk_mm,tracker_swim_lengths,tracker_pool_length,tracker_pool_length_unit,tracker_cardio_load,manually_logged_total_calories,manually_logged_total_steps,manually_logged_total_distance_mm,manually_logged_pool_length,manually_logged_pool_length_unit,events",
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
