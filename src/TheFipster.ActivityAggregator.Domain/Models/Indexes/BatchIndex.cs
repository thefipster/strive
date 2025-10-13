using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Files;

namespace TheFipster.ActivityAggregator.Domain.Models.Indexes;

public class BatchIndex
{
    public DateTime Timestamp { get; set; }
    public DataKind Kind { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public List<string> Parameters { get; set; } = [];
    public required string Filepath { get; set; }
    public required string Hash { get; set; }
    public List<DataSources> Sources { get; set; } = [];

    public DateTime IndexedAt { get; set; } = DateTime.UtcNow;
    public int Metrics { get; set; }
    public int Series { get; set; }
    public int Tracks { get; set; }
    public int Pulses { get; set; }
    public int Events { get; set; }
    public Dictionary<string, string> Assimilations { get; set; } = new();

    public static BatchIndex New(
        string filepath,
        string hash,
        MergedFile mergeFile,
        List<string> parameters,
        List<AssimilateIndex> assimilations
    )
    {
        return new BatchIndex
        {
            Timestamp = mergeFile.Timestamp,
            Kind = mergeFile.Kind,
            Sources = mergeFile.Sources,
            Start = mergeFile.Kind == DataKind.Day ? mergeFile.Timestamp.Date : mergeFile.Timestamp,
            End =
                mergeFile.Kind == DataKind.Day
                    ? mergeFile.Timestamp.Date.AddDays(1).AddMilliseconds(-1)
                    : mergeFile.Samples.Max(x => x.End),
            Series = mergeFile.Samples.Count,
            Tracks = mergeFile.Tracks.Count,
            Pulses = mergeFile.Pulses.Count,
            Events = mergeFile.Events?.Resolved.Count ?? 0,
            Parameters = parameters.Distinct().ToList(),
            Assimilations = assimilations
                .DistinctBy(x => x.Hash)
                .ToDictionary(x => x.Hash, y => y.Path),
            Filepath = filepath,
            Hash = hash,
        };
    }
}
