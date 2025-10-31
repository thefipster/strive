using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Ingestion.Components;

namespace Fip.Strive.Core.Ingestion.Vendors.Garmin.SleepData;

public class GarminSleepDataClassifier()
    : JsonClassifier(
        DataSources.GarminSleepData,
        DateRanges.Multi,
        [
            "sleepStartTimestampGMT",
            "calendarDate",
            "sleepEndTimestampGMT",
            "sleepWindowConfirmationType",
            "deepSleepSeconds",
        ],
        "calendarDate"
    );
