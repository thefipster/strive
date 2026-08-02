using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.Components;
using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Ingestion.Application.Vendors.Polar.Flow.Recovery;

public class PolarFlowNightlyRecoveryClassifier()
    : JsonClassifier(
        DataSources.PolarFlowNightlyRecovery,
        DateRanges.Multi,
        ["night", "sleepTip", "vitalityTip", "exerciseTip"],
        "night"
    );
