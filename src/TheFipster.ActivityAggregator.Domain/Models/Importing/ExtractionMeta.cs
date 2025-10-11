using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Files;

namespace TheFipster.ActivityAggregator.Domain.Models.Importing;

public class ExtractionMeta
{
    public required string Hash { get; set; }
    public required string Path { get; set; }
    public DateTime Timestamp { get; set; }
    public DateRanges Range { get; set; }
    public long Size { get; set; }

    public static ExtractionMeta New(
        FileExtraction extract,
        string extractFilepath,
        string hash,
        long size
    )
    {
        return new ExtractionMeta
        {
            Timestamp = extract.Timestamp,
            Path = extractFilepath,
            Range = extract.Range,
            Hash = hash,
            Size = size,
        };
    }
}
