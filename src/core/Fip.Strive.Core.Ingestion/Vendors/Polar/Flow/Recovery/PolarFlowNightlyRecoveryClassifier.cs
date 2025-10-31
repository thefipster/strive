using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Ingestion.Components;

namespace Fip.Strive.Core.Ingestion.Vendors.Polar.Flow.Recovery;

public class PolarFlowNightlyRecoveryClassifier()
    : JsonClassifier(
        DataSources.PolarFlowNightlyRecovery,
        DateRanges.Multi,
        ["night", "sleepTip", "vitalityTip", "exerciseTip"],
        "night"
    );
