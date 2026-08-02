using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Base;

namespace TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Vendors.Google.Timeline;

public class GoogleTimelineClassifier()
    : JsonClassifier(
        DataSources.GoogleTimeline,
        DateRanges.AllTime,
        ["locations", "timestampMs", "latitudeE7", "longitudeE7"]
    );
