namespace TheFipster.ActivityAggregator.Web.Models;

public class CommonIndex
{
    public DateTime? Timestamp { get; set; }
    public required string Path { get; set; }
    public required string TimeType { get; set; }
    public required string Type { get; set; }
    public DateTime IndexedAt { get; set; }
    public required string Hash { get; set; }
}
