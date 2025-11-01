using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Ingestion.Components;

namespace Fip.Strive.Core.Ingestion.Vendors.Google.Timeline.Takeout;

public class GoogleTimelineClassifier()
    : JsonClassifier(
        DataSources.GoogleTimeline,
        DateRanges.Multi,
        ["locations", "timestampMs", "latitudeE7", "longitudeE7"]
    );
