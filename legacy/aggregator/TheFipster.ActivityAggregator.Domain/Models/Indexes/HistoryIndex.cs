namespace TheFipster.ActivityAggregator.Domain.Models.Indexes;

public class HistoryIndex
{
    public required BatchIndex Batch { get; set; }
    public List<AssimilateIndex> Assimilations { get; set; } = [];
    public List<ExtractorIndex> Extractions { get; set; } = [];
    public List<FileIndex> Files { get; set; } = [];
    public List<ZipIndex> Zips { get; set; } = [];
}
