using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.Components;
using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Ingestion.Application.Vendors.Google.Fitbit.DailySpo2;

public class GoogleFitbitDailyOxygenSaturationClassifier()
    : CsvClassifier(
        DataSources.GoogleFitbitDailyOxygenSaturation,
        DateRanges.Multi,
        "timestamp,average percentage,lower bound percentage,upper bound percentage,baseline percentage,standard deviation percentage"
    );
