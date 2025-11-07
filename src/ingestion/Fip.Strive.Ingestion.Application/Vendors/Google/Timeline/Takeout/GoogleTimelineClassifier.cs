using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.Components;
using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Ingestion.Application.Vendors.Google.Timeline.Takeout;

public class GoogleTimelineClassifier()
    : JsonClassifier(
        DataSources.GoogleTimeline,
        DateRanges.Multi,
        ["locations", "timestampMs", "latitudeE7", "longitudeE7"]
    );
