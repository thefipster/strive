using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.Components;
using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Ingestion.Application.Vendors.Google.Timeline.Device;

public class GoogleTimelineDeviceClassifier()
    : JsonClassifier(
        DataSources.GoogleTimelineDevice,
        DateRanges.Multi,
        ["semanticSegments", "startTime", "endTime", "timelinePath"],
        "startTime"
    );
