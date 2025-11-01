using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Ingestion.Components;

namespace Fip.Strive.Core.Ingestion.Vendors.Google.Fitbit.HeartNotify;

public class GoogleFitbitHeartrateNotificationClassifier()
    : CsvClassifier(
        DataSources.GoogleFitbitHeartrateNotification,
        DateRanges.Multi,
        "timestamp,heart rate notification type,heart rate threshold beats per minute,heart rate trigger value beats per minute"
    );
