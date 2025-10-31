using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Ingestion.Components;

namespace Fip.Strive.Core.Ingestion.Vendors.Polar.Flow.RecoveryBlob;

public class PolarFlowNightlyRecoveryBlobClassifier()
    : JsonClassifier(
        DataSources.PolarFlowNightlyRecoveryBlob,
        DateRanges.Multi,
        ["hrvData", "startTime", "samplingIntervalInMillis", "samples"],
        "startTime"
    );
