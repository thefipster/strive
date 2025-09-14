namespace TheFipster.ActivityAggregator.Domain.Models.Indexes;

public class ImportIndex
{
    public ImportIndex(int version, string filepath, string hash)
    {
        Version = version;
        Filepath = filepath;
        Hash = hash;
    }

    public int Version { get; set; }
    public string Filepath { get; set; }
    public string Hash { get; set; }
}
