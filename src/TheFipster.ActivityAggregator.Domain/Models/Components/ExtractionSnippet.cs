namespace TheFipster.ActivityAggregator.Domain.Models.Components;

public class ExtractionSnippet
{
    public string Hash { get; set; }
    public DateTime Timestamp { get; set; }
    public DateRanges Range { get; set; }
    public string Path { get; set; }
}
