using TheFipster.ActivityAggregator.Domain.Models.Extraction;

namespace TheFipster.ActivityAggregator.Domain.Models.Indexes;

public class ClassificationIndex
{
    private IEnumerable<Classification> classifications = [];

    public ClassificationIndex() { }

    public ClassificationIndex(
        int version,
        string filepath,
        IEnumerable<Classification> classifications
    )
        : this()
    {
        Filepath = filepath;
        Version = version;
        Classifications = classifications as Classification[] ?? classifications.ToArray();
        IndexedAt = DateTime.UtcNow;
        SetClassification();
    }

    public string? Filepath { get; set; }
    public DateTime IndexedAt { get; set; }
    public int Version { get; set; }

    public IEnumerable<Classification> Classifications
    {
        get => classifications;
        set
        {
            classifications = value;
            SetClassification();
        }
    }

    // Computed values
    public string? Classification { get; set; }

    private void SetClassification()
    {
        Classification =
            Classifications.Count() == 1 ? Classifications.First().Source.ToString() : string.Empty;
    }
}
