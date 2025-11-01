using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;

namespace Fip.Strive.Core.Domain.Schemas.Index.Models;

public class FileIndex
{
    public required string Hash { get; set; }
    public Dictionary<string, DateTime> Files { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public required DateTime SignalledAt { get; set; }
    public required Guid SignalId { get; set; }
    public required Guid ReferenceId { get; set; }
    public required string ParentId { get; set; }
    public bool Classified { get; set; }
    public ClassificationResults ClassificationResult { get; set; }
    public string? ClassifierHash { get; set; }
    public DataSources? Source { get; set; }
    public int ClassfierVersion { get; set; }
    public DateTime? LastClassificationAt { get; set; }
    public DateTime? Timestamp { get; set; }
    public int ExtractorVersion { get; set; }
    public DateTime? LastExtractionAt { get; set; }
    public int Extractions { get; set; }
    public DateTime? ExtractionMinDate { get; set; }
    public DateTime? ExtractionMaxDate { get; set; }

    public void AddFile(string filename)
    {
        if (Files.ContainsKey(filename))
            return;

        Files.Add(filename, DateTime.UtcNow);
    }
}
