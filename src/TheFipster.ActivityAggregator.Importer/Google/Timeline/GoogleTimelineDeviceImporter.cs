using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Parser;

namespace TheFipster.ActivityAggregator.Importer.Google.Timeline;

public class GoogleTimelineDeviceImporter()
    : JsonClassifier(
        DataSources.GoogleTimelineDevice,
        DateRanges.AllTime,
        ["semanticSegments", "startTime", "endTime", "timelinePath"],
        "startTime"
    );
