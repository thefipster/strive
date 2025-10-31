using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Ingestion.Components;

namespace Fip.Strive.Core.Ingestion.Vendors.Google.Fitbit.ActiveZoneMinutes;

public class GoogleFitbitActiveZoneMinutesClassifier()
    : CsvClassifier(
        DataSources.GoogleFitbitActiveZoneMinutes,
        DateRanges.Multi,
        "timestamp,heart rate zone,total minutes"
    );
