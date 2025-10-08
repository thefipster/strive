using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Parser;

namespace TheFipster.ActivityAggregator.Importer.Google.Timeline;

public class GoogleTimelineImporter()
    : JsonClassifier(
        DataSources.GoogleTimeline,
        DateRanges.AllTime,
        ["locations", "timestampMs", "latitudeE7", "longitudeE7"]
    );
