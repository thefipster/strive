using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Parser;

namespace TheFipster.ActivityAggregator.Importer.Polar;

public class PolarFlowOrthostaticTestImporter()
    : JsonClassifier(
        DataSources.PolarFlowOrthostaticTest,
        DateRanges.Day,
        ["startTime", "orthostaticTestResult", "rrAvgSupine", "rrMinStandup"],
        "startTime"
    );
