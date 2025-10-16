using System.Text.Json;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Importing;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Tools;

namespace TheFipster.ActivityAggregator.Domain.Models.Files;

public class MergedFile
{
    public required DateTime Timestamp { get; set; }
    public required DataKind Kind { get; set; }
    public MetricMergeResult? Metrics { get; set; }
    public EventMergeResult? Events { get; set; }
    public Dictionary<string, string> Assimilations { get; set; } = new();
    public List<DataSources> Sources { get; set; } = [];
    public List<UnifiedSamples> Samples { get; set; } = [];
    public List<UnifiedTrack> Tracks { get; set; } = [];
    public List<List<double>> Pulses { get; set; } = [];

    public string Write(string rootDir)
    {
        var filename = $"{Timestamp.ToString(DateHelper.FsMillisecondFormat)}-{Kind}.json";
        var path = Path.Combine(rootDir, Timestamp.Year.ToString());
        var newFile = Path.Combine(path, filename);
        var json = JsonSerializer.Serialize(this);

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        if (!File.Exists(newFile))
            File.WriteAllText(newFile, json);

        return newFile;
    }

    public static MergedFile FromJson(string json)
    {
        var extract = JsonSerializer.Deserialize<MergedFile>(json);
        if (extract == null)
            throw new ArgumentException("Invalid json", nameof(json));

        return extract;
    }

    public static MergedFile FromFile(string filepath)
    {
        if (!File.Exists(filepath))
            throw new ArgumentException("File doesn't exist.", nameof(filepath));

        var json = File.ReadAllText(filepath);
        return FromJson(json);
    }

    public static MergedFile New(
        DateTime timestamp,
        DataKind kind,
        FileExtraction[] extracts,
        EventMergeResult eventMerge,
        NormalizedResult[] seriesMerge,
        MetricMergeResult metricsMerge,
        List<AssimilateIndex> assimilations
    )
    {
        var samples = seriesMerge.Where(x => x.Samples != null).Select(x => x.Samples!).ToList();
        var tracks = seriesMerge.Where(x => x.Track != null).Select(x => x.Track!).ToList();
        var pulses = seriesMerge.Where(x => x.Pulses != null).Select(x => x.Pulses!).ToList();

        return new MergedFile
        {
            Timestamp = timestamp,
            Kind = kind,
            Sources = extracts.Select(x => x.Source).ToList(),
            Events = eventMerge,
            Samples = samples,
            Tracks = tracks,
            Pulses = pulses,
            Metrics = metricsMerge,
            Assimilations = assimilations
                .DistinctBy(x => x.Hash)
                .ToDictionary(x => x.Hash, y => y.Path),
        };
    }
}
