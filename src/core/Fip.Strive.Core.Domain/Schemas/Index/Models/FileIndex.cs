using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;

namespace Fip.Strive.Core.Domain.Schemas.Index.Models;

public class FileIndex
{
    public required string Hash { get; set; }
    public Dictionary<string, DateTime> Files { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime SignalledAt { get; set; }
    public Guid SignalId { get; set; }
    public Guid ReferenceId { get; set; }

    public void AddFile(string filename)
    {
        if (Files.ContainsKey(filename))
            return;

        Files.Add(filename, DateTime.UtcNow);
    }
}
