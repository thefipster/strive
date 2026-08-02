using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.Components;
using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Ingestion.Application.Vendors.Google.Fitbit.Hrv;

public class GoogleFitbitHeartrateVariabilityClassifier()
    : CsvClassifier(
        DataSources.GoogleFitbitHeartrateVariability,
        DateRanges.Multi,
        "timestamp,root mean square of successive differences milliseconds,standard deviation milliseconds"
    );
