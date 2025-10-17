using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Base;

namespace TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Vendors.Polar.Flow;

public class PolarFlowNightlyRecoveryBlobClassifier()
    : JsonClassifier(
        DataSources.PolarFlowNightlyRecoveryBlob,
        DateRanges.AllTime,
        ["hrvData", "startTime", "samplingIntervalInMillis", "samples"],
        "startTime"
    );
