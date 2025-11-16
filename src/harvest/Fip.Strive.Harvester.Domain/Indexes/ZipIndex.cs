using Fip.Strive.Harvester.Domain.Signals;

namespace Fip.Strive.Harvester.Domain.Indexes;

public class ZipIndex
{
    public required string Filepath { get; set; }
    public required string Hash { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public static ZipIndex FromSignal(UploadSignal signal) =>
        new ZipIndex { Filepath = signal.Filepath, Hash = signal.Hash };
}
