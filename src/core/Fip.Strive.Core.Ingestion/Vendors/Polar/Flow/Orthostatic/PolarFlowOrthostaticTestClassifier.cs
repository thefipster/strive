using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Ingestion.Components;

namespace Fip.Strive.Core.Ingestion.Vendors.Polar.Flow.Orthostatic;

public class PolarFlowOrthostaticTestClassifier()
    : JsonClassifier(
        DataSources.PolarFlowOrthostaticTest,
        DateRanges.Day,
        ["startTime", "orthostaticTestResult", "rrAvgSupine", "rrMinStandup"],
        "startTime"
    );
