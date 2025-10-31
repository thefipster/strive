using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Ingestion.Components;

namespace Fip.Strive.Core.Ingestion.Vendors.Google.Fitbit.DailySpo2;

public class GoogleFitbitDailyOxygenSaturationClassifier()
    : CsvClassifier(
        DataSources.GoogleFitbitDailyOxygenSaturation,
        DateRanges.Multi,
        "timestamp,average percentage,lower bound percentage,upper bound percentage,baseline percentage,standard deviation percentage"
    );
