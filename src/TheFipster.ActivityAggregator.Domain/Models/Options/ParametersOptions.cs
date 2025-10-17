using TheFipster.ActivityAggregator.Domain.Enums;

namespace TheFipster.ActivityAggregator.Domain.Models.Options;

public static class ParametersOptions
{
    public const string Any = "Any";

    public static List<string> Items
    {
        get
        {
            var items = new List<string> { Any };
            items.AddRange(Enum.GetNames(typeof(Parameters)));
            return items;
        }
    }
}
