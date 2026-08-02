using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.Components;
using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Ingestion.Application.Vendors.Google.Fitbit.DemoVo2Max;

public class GoogleFitbitDemographicVo2MaxClassifier()
    : CsvClassifier(
        DataSources.GoogleFitbitDemographicVo2Max,
        DateRanges.Multi,
        "timestamp,demographic vo2max"
    );
