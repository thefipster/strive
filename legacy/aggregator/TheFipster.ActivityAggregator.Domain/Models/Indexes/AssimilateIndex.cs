using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Importing;

namespace TheFipster.ActivityAggregator.Domain.Models.Indexes;

public class AssimilateIndex
{
    public required string FileHash { get; set; }
    public required string Hash { get; set; }
    public required string Path { get; set; }
    public long Size { get; set; }
    public DateTime IndexedAt { get; set; } = DateTime.UtcNow;
    public DataSources Source { get; set; }
    public DateTime Timestamp { get; set; }
    public DataKind Kind { get; set; }
    public List<string> Metrics { get; set; } = [];

    public static AssimilateIndex New(ExtractionMeta extract, FileIndex file)
    {
        return new AssimilateIndex
        {
            Hash = extract.Hash,
            FileHash = file.Hash,
            Path = extract.Path,
            Timestamp = extract.Timestamp,
            Size = extract.Size,
            Kind = extract.Range == DateRanges.Time ? DataKind.Session : DataKind.Day,
            Source = file.Source!.Value,
        };
    }
}
