using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.Components;
using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Ingestion.Application.Vendors.Garmin.UdsFile;

public class GarminUdsFileClassifier()
    : JsonClassifier(
        DataSources.GarminUdsFile,
        DateRanges.Multi,
        ["userProfilePK", "uuid", "calendarDate", "durationInMilliseconds", "totalKilocalories"],
        "calendarDate"
    );
