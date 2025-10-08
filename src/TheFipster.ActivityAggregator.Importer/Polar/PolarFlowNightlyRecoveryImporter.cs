using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Parser;

namespace TheFipster.ActivityAggregator.Importer.Polar;

public class PolarFlowNightlyRecoveryImporter()
    : JsonClassifier(
        DataSources.PolarFlowNightlyRecovery,
        DateRanges.AllTime,
        ["night", "sleepTip", "vitalityTip", "exerciseTip"],
        "night"
    );
