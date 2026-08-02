using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.Components;
using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Ingestion.Application.Vendors.Google.Fitbit.DailyHrv;

public class GoogleFitbitDailyHeartrateVariabilityClassifier()
    : CsvClassifier(
        DataSources.GoogleFitbitDailyHeartrateVariability,
        DateRanges.Multi,
        "timestamp,average heart rate variability milliseconds,non rem heart rate beats per minute,entropy,deep sleep root mean square of successive differences milliseconds"
    );
