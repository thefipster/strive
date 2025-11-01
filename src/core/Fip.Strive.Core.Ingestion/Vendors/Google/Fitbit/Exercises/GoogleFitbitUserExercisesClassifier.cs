using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Ingestion.Components;

namespace Fip.Strive.Core.Ingestion.Vendors.Google.Fitbit.Exercises;

public class GoogleFitbitUserExercisesClassifier()
    : CsvClassifier(
        DataSources.GoogleFitbitUserExercises,
        DateRanges.Multi,
        "exercise_id,exercise_start,exercise_end,utc_offset,exercise_created,exercise_last_updated,activity_name,log_type,pool_length,pool_length_unit,intervals,distance_units,tracker_total_calories,tracker_total_steps,tracker_total_distance_mm,tracker_total_altitude_mm,tracker_avg_heart_rate,tracker_peak_heart_rate,tracker_avg_pace_mm_per_second,tracker_avg_speed_mm_per_second,tracker_peak_speed_mm_per_second,tracker_auto_stride_run_mm,tracker_auto_stride_walk_mm,tracker_swim_lengths,tracker_pool_length,tracker_pool_length_unit,tracker_cardio_load,manually_logged_total_calories,manually_logged_total_steps,manually_logged_total_distance_mm,manually_logged_pool_length,manually_logged_pool_length_unit,events"
    );
