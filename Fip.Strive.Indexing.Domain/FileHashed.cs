using System.ComponentModel.DataAnnotations;

namespace Fip.Strive.Indexing.Domain;

public class FileHashed
{
    public int Id { get; set; }

    public required string Hash { get; set; } = string.Empty;

    public required string FileName { get; set; } = string.Empty;

    public DateTime IndexedAt { get; set; }

    public virtual FileIndex? File { get; set; } = null!;
}
