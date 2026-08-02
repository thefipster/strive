using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.Components;
using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Ingestion.Application.Vendors.Google.Fitbit.HeartNotify;

public class GoogleFitbitHeartrateNotificationClassifier()
    : CsvClassifier(
        DataSources.GoogleFitbitHeartrateNotification,
        DateRanges.Multi,
        "timestamp,heart rate notification type,heart rate threshold beats per minute,heart rate trigger value beats per minute"
    );
