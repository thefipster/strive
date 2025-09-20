using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Extensions;

namespace TheFipster.ActivityAggregator.Domain.Models.Indexes;

public class TransformIndex
{
    public TransformIndex() { }

    public TransformIndex(
        int version,
        string filepath,
        string sourceFilepath,
        DataSources source,
        DateTime timestamp,
        DateRanges range
    )
    {
        Version = version;
        Filepath = filepath;
        SourceFilepath = sourceFilepath;
        Source = source;
        Timestamp = timestamp;
        Range = range;

        // Computed values
        Date = Timestamp.ToDateString();
        Time = Timestamp.ToTimeString();
        Filter = $"{Date}-{Range}";
    }

    public int Version { get; set; }
    public DateTime IndexedAt { get; set; } = DateTime.UtcNow;
    public string? Filepath { get; set; }
    public string? SourceFilepath { get; set; }
    public DataSources Source { get; set; }
    public DateTime Timestamp { get; set; }
    public DateRanges Range { get; set; }

    // Computed values
    public string? Date { get; set; }
    public string? Time { get; set; }
    public string? Filter { get; set; }
}
