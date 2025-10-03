namespace TheFipster.ActivityAggregator.Domain.Extensions;

public static class IndexExtensions
{
    public static void Log<T>(this Dictionary<DateTime, T> actions, T action)
    {
        var lastAction =
            actions.Count != 0 ? actions.OrderByDescending(x => x.Key).First().Value : default;

        if (lastAction == null || !lastAction.Equals(action))
            actions.Add(DateTime.UtcNow, action);
    }
}
