using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Ingestion.Components;

namespace Fip.Strive.Core.Ingestion.Vendors.Polar.Flow.SleepWake;

public class PolarFlowSleepWakeClassifier()
    : JsonClassifier(
        DataSources.PolarFlowSleepWake,
        DateRanges.Multi,
        ["night", "sleepWake", "sleepStateChanges", "sleepWakeStateChangeModels"],
        "night"
    );
