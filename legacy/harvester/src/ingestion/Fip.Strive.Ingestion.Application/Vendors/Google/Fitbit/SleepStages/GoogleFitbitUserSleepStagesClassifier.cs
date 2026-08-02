using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.Components;
using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Ingestion.Application.Vendors.Google.Fitbit.SleepStages;

public class GoogleFitbitUserSleepStagesClassifier()
    : CsvClassifier(
        DataSources.GoogleFitbitUserSleepStages,
        DateRanges.Multi,
        "sleep_id,sleep_stage_id,sleep_stage_type,start_utc_offset,sleep_stage_start,end_utc_offset,sleep_stage_end,data_source,sleep_stage_created,sleep_stage_last_updated"
    );
