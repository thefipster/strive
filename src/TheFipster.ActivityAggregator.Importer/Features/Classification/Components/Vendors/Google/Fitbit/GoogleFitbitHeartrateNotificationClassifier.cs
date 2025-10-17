using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Base;

namespace TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Vendors.Google.Fitbit;

public class GoogleFitbitHeartrateNotificationClassifier()
    : CsvClassifier(
        DataSources.GoogleFitbitHeartrateNotification,
        DateRanges.Month,
        "timestamp,heart rate notification type,heart rate threshold beats per minute,heart rate trigger value beats per minute"
    );
