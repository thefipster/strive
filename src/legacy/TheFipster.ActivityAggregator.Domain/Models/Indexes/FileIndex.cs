using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Importing;

namespace TheFipster.ActivityAggregator.Domain.Models.Indexes;

public class FileIndex
{
    public required string ZipHash { get; set; }
    public required string Hash { get; set; }
    public required string Path { get; set; }
    public List<string> AlternateFiles { get; set; } = new();
    public long Size { get; set; }
    public DateTime IndexedAt { get; set; } = DateTime.UtcNow;
    public DataSources? Source { get; set; }
    public DateTime? Timestamp { get; set; }
    public DateRanges? Range { get; set; }

    public static FileIndex New(string hash, string zipHash, long size, string path)
    {
        return new FileIndex
        {
            Hash = hash,
            ZipHash = zipHash,
            Size = size,
            Path = path,
        };
    }

    public void SetClassification(Classification? classification)
    {
        Source = classification?.Source;
        Timestamp = classification?.Datetime;
        Range = classification?.Range;
    }
}
