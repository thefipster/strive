using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Ingestion.Components;

namespace Fip.Strive.Core.Ingestion.Vendors.Google.Fitbit.Hrv;

public class GoogleFitbitHeartrateVariabilityClassifier()
    : CsvClassifier(
        DataSources.GoogleFitbitHeartrateVariability,
        DateRanges.Multi,
        "timestamp,root mean square of successive differences milliseconds,standard deviation milliseconds"
    );
