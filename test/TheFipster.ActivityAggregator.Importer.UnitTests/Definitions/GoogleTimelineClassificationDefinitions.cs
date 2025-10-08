using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Google.Fitbit;
using TheFipster.ActivityAggregator.Importer.Google.Timeline;
using TheFipster.ActivityAggregator.Importer.UnitTests.Models;

namespace TheFipster.ActivityAggregator.Importer.UnitTests.Definitions;

public static class GoogleTimelineClassificationDefinitions
{
    public static IEnumerable<object[]> GetClassificationCases()
    {
        yield return
        [
            new FileClassificationCase(
                typeof(GoogleTimelineImporter),
                Vendors.GoogleTimeline,
                "2019.09.09 - Timeline.json",
                "0001.01.01",
                DateRanges.AllTime,
                DataSources.GoogleTimeline
            ),
        ];

        yield return
        [
            new FileClassificationCase(
                typeof(GoogleTimelineDeviceImporter),
                Vendors.GoogleTimeline,
                "2025.09.17 - Timeline.json",
                "0001.01.01",
                DateRanges.AllTime,
                DataSources.GoogleTimelineDevice
            ),
        ];
    }
}
