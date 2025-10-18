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
        var index = new BatchIndex
        {
            Filepath = filepath,
            Hash = hash,
            Timestamp = mergeFile.Timestamp,
            Kind = mergeFile.Kind,
            Sources = mergeFile.Sources,
        };

        index.Start = GetStartTimestamp(mergeFile);
        index.End = GetEndTimestamp(mergeFile);
        index.Series = mergeFile.Samples.Count;
        index.Tracks = mergeFile.Tracks.Count;
        index.Pulses = mergeFile.Pulses.Count;
        index.Events = mergeFile.Events?.Resolved.Count ?? 0;
        index.Parameters = parameters.Distinct().ToList();
        index.Assimilations = assimilations
            .DistinctBy(x => x.Hash)
            .ToDictionary(x => x.Hash, y => y.Path);

        return index;
    }

    private static DateTime GetStartTimestamp(MergedFile mergeFile)
    {
        if (mergeFile.Kind == DataKind.Day)
            return mergeFile.Timestamp.Date;

        return mergeFile.Timestamp;
    }

    private static DateTime GetEndTimestamp(MergedFile mergeFile)
    {
        if (mergeFile.Kind == DataKind.Day)
            return mergeFile.Timestamp.Date.AddDays(1).AddMilliseconds(-1);

        if (mergeFile.Samples.Any())
            return mergeFile.Samples.Max(x => x.End);

        return mergeFile.Timestamp;
    }
}
