using System.ComponentModel.DataAnnotations;

namespace Fip.Strive.Indexing.Domain;

public class ZipHashed
{
    public int Id { get; set; }

    public required string Hash { get; set; } = string.Empty;

    public required string FileName { get; set; } = string.Empty;

    public DateTime IndexedAt { get; set; }

    public virtual ZipIndex? Zip { get; set; } = null!;
}
