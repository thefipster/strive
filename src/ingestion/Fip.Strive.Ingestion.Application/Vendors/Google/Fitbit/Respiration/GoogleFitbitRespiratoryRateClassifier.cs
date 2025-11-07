using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.Components;
using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Ingestion.Application.Vendors.Google.Fitbit.Respiration;

public class GoogleFitbitRespiratoryRateClassifier()
    : CsvClassifier(
        DataSources.GoogleFitbitRespiratoryRate,
        DateRanges.Multi,
        "timestamp,breaths per minute"
    );
