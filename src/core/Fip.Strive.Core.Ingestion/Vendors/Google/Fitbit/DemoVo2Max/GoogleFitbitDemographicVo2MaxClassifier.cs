using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Ingestion.Components;

namespace Fip.Strive.Core.Ingestion.Vendors.Google.Fitbit.DemoVo2Max;

public class GoogleFitbitDemographicVo2MaxClassifier()
    : CsvClassifier(
        DataSources.GoogleFitbitDemographicVo2Max,
        DateRanges.Multi,
        "timestamp,demographic vo2max"
    );
