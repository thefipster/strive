using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Parser;

namespace TheFipster.ActivityAggregator.Importer.Garmin;

public class GarminUdsFileImporter()
    : JsonClassifier(
        DataSources.GarminUdsFile,
        DateRanges.AllTime,
        ["userProfilePK", "uuid", "calendarDate", "durationInMilliseconds", "totalKilocalories"],
        "calendarDate"
    );
