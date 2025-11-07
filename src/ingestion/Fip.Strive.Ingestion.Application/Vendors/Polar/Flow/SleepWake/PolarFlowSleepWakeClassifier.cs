using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.Components;
using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Ingestion.Application.Vendors.Polar.Flow.SleepWake;

public class PolarFlowSleepWakeClassifier()
    : JsonClassifier(
        DataSources.PolarFlowSleepWake,
        DateRanges.Multi,
        ["night", "sleepWake", "sleepStateChanges", "sleepWakeStateChangeModels"],
        "night"
    );
