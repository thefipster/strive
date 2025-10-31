using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Ingestion.Components;

namespace Fip.Strive.Core.Ingestion.Vendors.Google.Fitbit.CaloriesZone;

public class GoogleFitbitCaloriesHeartrateZoneClassifier()
    : CsvClassifier(
        DataSources.GoogleFitbitCaloriesHeartrateZone,
        DateRanges.Multi,
        "timestamp,heart rate zone type,kcal"
    );
