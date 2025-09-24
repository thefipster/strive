using TheFipster.ActivityAggregator.Domain.Enums;

namespace TheFipster.ActivityAggregator.Domain.Extensions;

public static class IndexExtensions
{
    public static void Log(
        this Dictionary<DateTime, ScannerActions> actions,
        ScannerActions action
    ) => actions.Add(DateTime.UtcNow, action);
}
