namespace Fip.Strive.Indexing.Domain;

public class FileHashed
{
    public int Id { get; set; }

    public required string Hash { get; set; }

    public required string FileName { get; set; }

    public DateTime IndexedAt { get; set; }

    public virtual FileIndex? File { get; set; }
}
