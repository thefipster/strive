namespace TheFipster.ActivityAggregator.Domain.Models.Indexes;

public class ClassificationIndex
{
    public ClassificationIndex(
        int version,
        string filepath,
        IEnumerable<Classification> classifications
    )
    {
        Version = version;
        Filepath = filepath;
        Classifications = classifications;
    }

    public string Filepath { get; set; }
    public int Version { get; set; }
    public IEnumerable<Classification> Classifications { get; set; }
}
