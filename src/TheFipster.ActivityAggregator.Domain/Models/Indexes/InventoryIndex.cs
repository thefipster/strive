using TheFipster.ActivityAggregator.Domain.Enums;

namespace TheFipster.ActivityAggregator.Domain.Models.Indexes;

public class InventoryIndex
{
    public InventoryIndex() { }

    public InventoryIndex(DateTime timestamp, DateRanges range)
    {
        if (range != DateRanges.Time && range != DateRanges.Day)
            throw new ArgumentException("Range must be day or time.", nameof(range));

        Timestamp = range == DateRanges.Time ? timestamp : timestamp.Date;
        IsDay = range != DateRanges.Time;
        Count = 1;
    }

    public DateTime Timestamp { get; set; }
    public bool IsDay { get; set; }
    public int Count { get; set; }
}
