using TheFipster.ActivityAggregator.Domain.Enums;

namespace TheFipster.ActivityAggregator.Domain.Models.Importing;

public class ExtractionSnippet
{
    public required string Hash { get; set; }
    public required string Path { get; set; }
    public DateTime Timestamp { get; set; }
    public DateRanges Range { get; set; }
    public long Size { get; set; }
}
