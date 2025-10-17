using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Parser;

namespace TheFipster.ActivityAggregator.Importer.Polar;

public class PolarFlowNightlyRecoveryBlobImporter()
    : JsonClassifier(
        DataSources.PolarFlowNightlyRecoveryBlob,
        DateRanges.AllTime,
        ["hrvData", "startTime", "samplingIntervalInMillis", "samples"],
        "startTime"
    );
