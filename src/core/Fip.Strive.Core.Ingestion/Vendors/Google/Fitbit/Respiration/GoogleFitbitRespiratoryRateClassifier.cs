using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Ingestion.Components;

namespace Fip.Strive.Core.Ingestion.Vendors.Google.Fitbit.Respiration;

public class GoogleFitbitRespiratoryRateClassifier()
    : CsvClassifier(
        DataSources.GoogleFitbitRespiratoryRate,
        DateRanges.AllTime,
        "timestamp,breaths per minute"
    );
