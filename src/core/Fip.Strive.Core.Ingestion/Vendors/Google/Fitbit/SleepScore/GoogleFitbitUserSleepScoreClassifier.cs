using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Ingestion.Components;

namespace Fip.Strive.Core.Ingestion.Vendors.Google.Fitbit.SleepScore;

public class GoogleFitbitUserSleepScoreClassifier()
    : CsvClassifier(
        DataSources.GoogleFitbitUserSleepScore,
        DateRanges.Multi,
        "sleep_id,sleep_score_id,data_source,score_utc_offset,score_time,overall_score,duration_score,composition_score,revitalization_score,sleep_time_minutes,deep_sleep_minutes,rem_sleep_percent,resting_heart_rate,sleep_goal_minutes,waso_count_long_wakes,waso_count_all_wake_time,restlessness_normalized,hr_below_resting_hr,sleep_score_created,sleep_score_last_updated"
    );
