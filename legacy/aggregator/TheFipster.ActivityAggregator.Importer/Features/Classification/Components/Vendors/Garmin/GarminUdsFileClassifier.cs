using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Base;

namespace TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Vendors.Garmin;

public class GarminUdsFileClassifier()
    : JsonClassifier(
        DataSources.GarminUdsFile,
        DateRanges.AllTime,
        ["userProfilePK", "uuid", "calendarDate", "durationInMilliseconds", "totalKilocalories"],
        "calendarDate"
    );
