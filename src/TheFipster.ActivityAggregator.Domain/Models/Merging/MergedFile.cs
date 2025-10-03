using System.Text.Json;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Extensions;
using TheFipster.ActivityAggregator.Domain.Models.Unified;
using TheFipster.ActivityAggregator.Domain.Tools;

namespace TheFipster.ActivityAggregator.Domain.Models.Merging;

public class MergedFile
{
    public required DateTime Timestamp { get; set; }
    public required DataKind Kind { get; set; }
    public MetricMergeResult? Metrics { get; set; }
    public EventMergeResult? Events { get; set; }
    public Dictionary<string, string> Extractions { get; set; } = new();
    public List<DataSources> Sources { get; set; } = [];
    public List<UnifiedSamples?> Series { get; set; } = [];
    public List<UnifiedTrack?> Tracks { get; set; } = [];
    public List<List<double>?> Pulses { get; set; } = [];

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
}
