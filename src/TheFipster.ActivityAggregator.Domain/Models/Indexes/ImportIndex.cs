namespace TheFipster.ActivityAggregator.Domain.Models.Indexes;

public class ImportIndex(int version, string import, string output)
{
    public int Version { get; set; } = version;
    public DateTime IndexedAt { get; set; } = DateTime.UtcNow;
    public string ImportPath { get; set; } = import;
    public string OutputDirectory { get; set; } = output;
}
