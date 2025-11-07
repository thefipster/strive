namespace Fip.Strive.Indexing.Domain;

public class ZipIndex
{
    public required string Hash { get; set; }

    public ICollection<ZipHashed> Files { get; set; } = [];

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime SignalledAt { get; set; }

    public Guid SignalId { get; set; }

    public Guid ReferenceId { get; set; }

    public virtual ICollection<FileIndex> Children { get; set; } = [];
}
