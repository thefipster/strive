using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Base;

namespace TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Vendors.Google.Fitbit;

public class GoogleFitbitActiveZoneMinutesClassifier()
    : CsvClassifier(
        DataSources.GoogleFitbitActiveZoneMinutes,
        DateRanges.Month,
        "timestamp,heart rate zone,total minutes"
    );
