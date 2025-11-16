namespace Fip.Strive.Indexing.Domain.Models;

public class ZipHashed
{
    public int Id { get; set; }

    public required string Hash { get; set; } = string.Empty;

    public required string FileName { get; set; } = string.Empty;

    public DateTime IndexedAt { get; set; }

    public virtual ZipIndex? Zip { get; set; }
}
