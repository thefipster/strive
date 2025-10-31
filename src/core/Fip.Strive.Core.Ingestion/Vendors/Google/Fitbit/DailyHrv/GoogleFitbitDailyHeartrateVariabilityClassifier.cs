using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Ingestion.Components;

namespace Fip.Strive.Core.Ingestion.Vendors.Google.Fitbit.DailyHrv;

public class GoogleFitbitDailyHeartrateVariabilityClassifier()
    : CsvClassifier(
        DataSources.GoogleFitbitDailyHeartrateVariability,
        DateRanges.Multi,
        "timestamp,average heart rate variability milliseconds,non rem heart rate beats per minute,entropy,deep sleep root mean square of successive differences milliseconds"
    );
