using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Google;

public class GoogleHeartrateNotificationImporter()
    : GoogleCsvParser(
        DataSources.FitbitTakeoutHeartrateNotification,
        DateRanges.Month,
        "timestamp,heart rate notification type,heart rate threshold beats per minute,heart rate trigger value beats per minute"
    ),
        IFileClassifier;
