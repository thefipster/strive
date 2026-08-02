using Fip.Strive.Harvester.Application.Core.Signals;

namespace Fip.Strive.Harvester.Application.Infrastructure.Indexing.Data;

public class ZipIndex
{
    public required string Filepath { get; set; }
    public required string Hash { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<FileInstance> Files { get; set; } = [];

    public static ZipIndex FromSignal(UploadSignal signal, string path) =>
        new ZipIndex { Filepath = path, Hash = signal.Hash };
}
