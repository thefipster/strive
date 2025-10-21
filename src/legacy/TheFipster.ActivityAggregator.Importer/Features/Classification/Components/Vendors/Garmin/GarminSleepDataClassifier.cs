using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Base;

namespace TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Vendors.Garmin;

public class GarminSleepDataClassifier()
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
