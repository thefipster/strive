using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Ingestion.Components;

namespace Fip.Strive.Core.Ingestion.Vendors.Google.Fitbit.SleepStages;

public class GoogleFitbitUserSleepStagesClassifier()
    : CsvClassifier(
        DataSources.GoogleFitbitUserSleepStages,
        DateRanges.Multi,
        "sleep_id,sleep_stage_id,sleep_stage_type,start_utc_offset,sleep_stage_start,end_utc_offset,sleep_stage_end,data_source,sleep_stage_created,sleep_stage_last_updated"
    );
