using Fip.Strive.Harvester.Domain.Signals;

namespace Fip.Strive.Harvester.Domain.Indexes;

public class FileInstance
{
    public required string Filepath { get; set; }
    public required string ParentFilepath { get; set; }
    public required string Hash { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public static FileInstance From(string file, string hash, ScanSignal inSignal) =>
        new FileInstance
        {
            Filepath = file,
            Hash = hash,
            ParentFilepath = Path.GetFileName(inSignal.ZipPath),
        };
}
