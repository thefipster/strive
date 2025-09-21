using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Google;

public class GoogleActiveZoneMinutesImporter()
    : GoogleCsvParser(
        DataSources.FitbitTakeoutActiveZoneMinutes,
        DateRanges.Month,
        "timestamp,heart rate zone,total minutes"
    ),
        IFileClassifier;
