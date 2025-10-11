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
    public List<ExtractionMeta> ExtractedFiles { get; set; } = new();
    public List<string> Metrics { get; set; } = [];

    public static ExtractorIndex New(
        FileIndex file,
        string valueHash,
        List<ExtractionMeta> extracts,
        List<string> metrics,
        long size
    )
    {
        return new ExtractorIndex
        {
            Path = file.Path,
            FileHash = file.Hash,
            ValueHash = valueHash,
            ZipHash = file.ZipHash,
            Source = file.Source,
            Range = file.Range,
            Timestamp = file.Timestamp,
            ExtractedFiles = extracts,
            Metrics = metrics.Distinct().ToList(),
            Size = size,
        };
    }
}
