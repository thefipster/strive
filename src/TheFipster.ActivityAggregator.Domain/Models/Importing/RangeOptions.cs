using TheFipster.ActivityAggregator.Domain.Enums;

namespace TheFipster.ActivityAggregator.Domain.Models.Importing;

public class RangeOptions
{
    public const string All = "All";

    public static List<string> Items
    {
        get
        {
            var items = new List<string> { All };
            items.AddRange(Enum.GetNames(typeof(DateRanges)));
            return items;
        }
    }
}
