using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Ingestion.Components;

namespace Fip.Strive.Core.Ingestion.Vendors.Garmin.UdsFile;

public class GarminUdsFileClassifier()
    : JsonClassifier(
        DataSources.GarminUdsFile,
        DateRanges.Multi,
        ["userProfilePK", "uuid", "calendarDate", "durationInMilliseconds", "totalKilocalories"],
        "calendarDate"
    );
