using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.Components;
using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Ingestion.Application.Vendors.Garmin.SleepData;

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
