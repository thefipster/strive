using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Base;

namespace TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Vendors.Polar.Flow;

public class PolarFlowNightlyRecoveryClassifier()
    : JsonClassifier(
        DataSources.PolarFlowNightlyRecovery,
        DateRanges.AllTime,
        ["night", "sleepTip", "vitalityTip", "exerciseTip"],
        "night"
    );
