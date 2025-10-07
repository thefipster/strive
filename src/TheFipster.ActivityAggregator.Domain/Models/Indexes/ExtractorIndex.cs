using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Importing;

namespace TheFipster.ActivityAggregator.Domain.Models.Indexes;

public class ExtractorIndex
{
    public required string ZipHash { get; set; }
    public required string FileHash { get; set; }
    public required string ValueHash { get; set; }
    public required string Path { get; set; }
    public long Size { get; set; }
    public DateTime IndexedAt { get; set; } = DateTime.UtcNow;
    public DataSources? Source { get; set; }
    public DateTime? Timestamp { get; set; }
    public DateRanges? Range { get; set; }
    public List<ExtractionSnippet> ExtractedFiles { get; set; } = new();
    public List<string> Metrics { get; set; } = [];
}
