using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;

namespace Fip.Strive.Core.Domain.Schemas.Index.Models;

public class ZipIndex
{
    public required string Hash { get; set; }
    public Dictionary<string, DateTime> Files { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid SignalId { get; set; }
    public Guid ReferenceId { get; set; }

    public void AddFile(string filename) => Files.Add(filename, DateTime.UtcNow);

    public static ZipIndex From(Signal signal, string hash)
    {
        return new()
        {
            SignalId = signal.Id,
            ReferenceId = signal.ReferenceId,
            Hash = hash,
        };
    }
}
