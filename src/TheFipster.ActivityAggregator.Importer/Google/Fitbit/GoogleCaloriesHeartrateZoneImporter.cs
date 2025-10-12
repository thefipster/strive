using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Google.Fitbit;

public class GoogleCaloriesHeartrateZoneImporter()
    : GoogleCsvParser(
        DataSources.GoogleFitbitCaloriesHeartrateZone,
        DateRanges.Month,
        "timestamp,heart rate zone type,kcal"
    ),
        IFileClassifier;
