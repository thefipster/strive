using TheFipster.ActivityAggregator.Domain.Enums;

namespace TheFipster.ActivityAggregator.Domain.Models.Indexes;

public class InventoryIndex
{
    public DateTime Timestamp { get; set; }
    public bool IsDay { get; set; }

    public static InventoryIndex Parse(ConvergeIndex index)
    {
        return new InventoryIndex
        {
            Timestamp = index.Kind == DataKind.Day ? index.Timestamp.Date : index.Timestamp,
            IsDay = index.Kind == DataKind.Day,
        };
    }
}
