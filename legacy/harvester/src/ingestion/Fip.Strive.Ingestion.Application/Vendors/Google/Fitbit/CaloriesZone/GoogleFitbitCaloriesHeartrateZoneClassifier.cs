using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.Components;
using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Ingestion.Application.Vendors.Google.Fitbit.CaloriesZone;

public class GoogleFitbitCaloriesHeartrateZoneClassifier()
    : CsvClassifier(
        DataSources.GoogleFitbitCaloriesHeartrateZone,
        DateRanges.Multi,
        "timestamp,heart rate zone type,kcal"
    );
