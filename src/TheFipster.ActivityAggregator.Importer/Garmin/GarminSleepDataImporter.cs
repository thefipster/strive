using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Parser;

namespace TheFipster.ActivityAggregator.Importer.Garmin;

public class GarminSleepDataImporter()
    : JsonClassifier(
        DataSources.GarminSleepData,
        DateRanges.AllTime,
        [
            "sleepStartTimestampGMT",
            "calendarDate",
            "sleepEndTimestampGMT",
            "sleepWindowConfirmationType",
            "deepSleepSeconds",
        ],
        "calendarDate"
    );
