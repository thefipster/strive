using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Base;

namespace TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Vendors.Google.Fitbit;

public class GoogleFitbitUserSleepStagesClassifier()
    : CsvClassifier(
        DataSources.GoogleFitbitUserSleepStages,
        DateRanges.AllTime,
        "sleep_id,sleep_stage_id,sleep_stage_type,start_utc_offset,sleep_stage_start,end_utc_offset,sleep_stage_end,data_source,sleep_stage_created,sleep_stage_last_updated"
    );
