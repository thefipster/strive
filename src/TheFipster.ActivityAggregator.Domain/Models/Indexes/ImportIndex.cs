namespace TheFipster.ActivityAggregator.Domain.Models.Indexes;

public class ImportIndex
{
    public ImportIndex() { }

    public ImportIndex(int version, string import, string output)
    {
        Version = version;
        ImportPath = import;
        OutputDirectory = output;
    }

    public int Version { get; set; }
    public DateTime IndexedAt { get; set; } = DateTime.UtcNow;
    public string? ImportPath { get; set; }
    public string? OutputDirectory { get; set; }
}
