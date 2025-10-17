using TheFipster.ActivityAggregator.Domain.Enums;

namespace TheFipster.ActivityAggregator.Domain.Models.Options;

public static class ClassificationOptions
{
    public const string All = "All";
    public const string Classified = "Classified";
    public const string Unclassified = "Unclassified";

    public static List<string> Items
    {
        get
        {
            var items = new List<string> { All, Classified, Unclassified };
            items.AddRange(Enum.GetNames(typeof(DataSources)));
            return items;
        }
    }
}
