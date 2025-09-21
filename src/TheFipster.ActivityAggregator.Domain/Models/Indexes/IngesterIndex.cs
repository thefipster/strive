using System.Text.Json.Serialization;

namespace TheFipster.ActivityAggregator.Domain.Models.Indexes;

public class IngesterIndex
{
    public IngesterIndex() { }

    public IngesterIndex(
        string inputFile,
        ScanIndex scanIndex,
        ClassificationIndex classificationIndex
    )
    {
        Filepath = inputFile;
        ImportDirectory = scanIndex.Directory;
        Hash = scanIndex.Hash;
        Classification = classificationIndex.Classification;
        ScannerVersion = scanIndex.Version;
        ScannedAt = scanIndex.IndexedAt;
        ClassifierVersion = classificationIndex.Version;
        ClassifiedAt = classificationIndex.IndexedAt;
    }

    public string? Filepath { get; set; }

    public string? ImportDirectory { get; set; }

    public string? Hash { get; set; }

    public string? Classification { get; set; }

    public int ScannerVersion { get; set; }

    public DateTime ScannedAt { get; set; }

    public int ClassifierVersion { get; set; }

    public DateTime ClassifiedAt { get; set; }
}
