using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.Components;
using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Ingestion.Application.Vendors.Google.Fitbit.ActiveZoneMinutes;

public class GoogleFitbitActiveZoneMinutesClassifier()
    : CsvClassifier(
        DataSources.GoogleFitbitActiveZoneMinutes,
        DateRanges.Multi,
        "timestamp,heart rate zone,total minutes"
    );
