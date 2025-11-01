using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Ingestion.Components;

namespace Fip.Strive.Core.Ingestion.Vendors.Google.Timeline.Device;

public class GoogleTimelineDeviceClassifier()
    : JsonClassifier(
        DataSources.GoogleTimelineDevice,
        DateRanges.Multi,
        ["semanticSegments", "startTime", "endTime", "timelinePath"],
        "startTime"
    );
