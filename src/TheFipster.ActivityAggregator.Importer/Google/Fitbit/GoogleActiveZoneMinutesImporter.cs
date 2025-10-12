using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Google.Fitbit;

public class GoogleActiveZoneMinutesImporter()
    : GoogleCsvParser(
        DataSources.GoogleFitbitActiveZoneMinutes,
        DateRanges.Month,
        "timestamp,heart rate zone,total minutes"
    ),
        IFileClassifier;
