using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Base;

namespace TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Vendors.Polar.Flow;

public class PolarFlowOrthostaticTestClassifier()
    : JsonClassifier(
        DataSources.PolarFlowOrthostaticTest,
        DateRanges.Day,
        ["startTime", "orthostaticTestResult", "rrAvgSupine", "rrMinStandup"],
        "startTime"
    );
