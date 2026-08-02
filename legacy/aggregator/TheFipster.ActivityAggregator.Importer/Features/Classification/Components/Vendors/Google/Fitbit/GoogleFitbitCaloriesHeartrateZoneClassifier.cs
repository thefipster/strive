using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Base;

namespace TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Vendors.Google.Fitbit;

public class GoogleFitbitCaloriesHeartrateZoneClassifier()
    : CsvClassifier(
        DataSources.GoogleFitbitCaloriesHeartrateZone,
        DateRanges.Month,
        "timestamp,heart rate zone type,kcal"
    );
