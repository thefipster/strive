using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.Components;
using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Ingestion.Application.Vendors.Polar.Flow.RecoveryBlob;

public class PolarFlowNightlyRecoveryBlobClassifier()
    : JsonClassifier(
        DataSources.PolarFlowNightlyRecoveryBlob,
        DateRanges.Multi,
        ["hrvData", "startTime", "samplingIntervalInMillis", "samples"],
        "startTime"
    );
