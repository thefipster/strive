namespace TheFipster.ActivityAggregator.Domain.Models.Indexes;

public class IngesterIndex(
    string inputFile,
    ScanIndex scanIndex,
    ClassificationIndex classificationIndex
)
{
    public string Filepath { get; } = inputFile;
    public string? ImportDirectory { get; } = scanIndex.Directory;
    public string? Hash { get; } = scanIndex.Hash;
    public string? Classification { get; } = classificationIndex.Classification;

    public int ScannerVersion { get; } = scanIndex.Version;
    public DateTime ScannedAt { get; } = scanIndex.IndexedAt;

    public int ClassifierVersion { get; } = classificationIndex.Version;
    public DateTime ClassifiedAt { get; } = classificationIndex.IndexedAt;
}
