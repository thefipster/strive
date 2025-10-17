using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Base;

namespace TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Vendors.Polar.Flow;

public class PolarFlowSleepWakeClassifier()
    : JsonClassifier(
        DataSources.PolarFlowSleepWake,
        DateRanges.AllTime,
        ["night", "sleepWake", "sleepStateChanges", "sleepWakeStateChangeModels"],
        "night"
    );
