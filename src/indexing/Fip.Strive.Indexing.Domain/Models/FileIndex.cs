using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Indexing.Domain.Models;

public class FileIndex
{
    public required string Hash { get; set; }
    public ICollection<FileHashed> Files { get; set; } = [];

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

    public virtual ZipIndex? Parent { get; set; }
    public virtual ICollection<DataIndex> Children { get; set; } = [];
}
