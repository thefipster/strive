using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Parser;

namespace TheFipster.ActivityAggregator.Importer.Polar;

public class PolarFlowSleepWakeImporter()
    : JsonClassifier(
        DataSources.PolarFlowSleepWake,
        DateRanges.AllTime,
        ["night", "sleepWake", "sleepStateChanges", "sleepWakeStateChangeModels"],
        "night"
    );
