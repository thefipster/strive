namespace TheFipster.ActivityAggregator.Domain.Models.Indexes;

public class ClassificationIndex
{
    public ClassificationIndex(
        int version,
        string filepath,
        IEnumerable<Classification> classifications
    )
    {
        Filepath = filepath;
        Version = version;
        Classifications = classifications as Classification[] ?? classifications.ToArray();
        IndexedAt = DateTime.UtcNow;

        Filter =
            Classifications.Count() == 1 ? Classifications.First().Source.ToString() : string.Empty;
    }

    public string Filepath { get; set; }
    public DateTime IndexedAt { get; set; }
    public int Version { get; set; }
    public IEnumerable<Classification> Classifications { get; set; }

    // Computed values
    public string Filter { get; set; }
}
