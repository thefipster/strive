using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Base;

namespace TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Vendors.Google.Timeline;

public class GoogleTimelineDeviceClassifier()
    : JsonClassifier(
        DataSources.GoogleTimelineDevice,
        DateRanges.AllTime,
        ["semanticSegments", "startTime", "endTime", "timelinePath"],
        "startTime"
    );
