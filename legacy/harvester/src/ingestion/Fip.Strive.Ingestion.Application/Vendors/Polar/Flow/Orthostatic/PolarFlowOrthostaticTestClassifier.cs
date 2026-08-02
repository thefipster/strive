using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.Components;
using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Ingestion.Application.Vendors.Polar.Flow.Orthostatic;

public class PolarFlowOrthostaticTestClassifier()
    : JsonClassifier(
        DataSources.PolarFlowOrthostaticTest,
        DateRanges.Day,
        ["startTime", "orthostaticTestResult", "rrAvgSupine", "rrMinStandup"],
        "startTime"
    );
