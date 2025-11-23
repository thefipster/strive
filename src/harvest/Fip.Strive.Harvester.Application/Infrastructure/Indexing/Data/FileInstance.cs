using Fip.Strive.Harvester.Application.Core.Signals;

namespace Fip.Strive.Harvester.Application.Infrastructure.Indexing.Data;

public class FileInstance
{
    public required string Filepath { get; set; }
    public required string ParentFilepath { get; set; }
    public required string Hash { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual ZipIndex? ParentZip { get; set; }

    public virtual SourceIndex? Source { get; set; }

    public virtual ExtractIndex? Extract { get; set; }

    public static FileInstance From(string file, string hash, ScanSignal inSignal) =>
        new FileInstance
        {
            Filepath = file,
            Hash = hash,
            ParentFilepath = inSignal.ZipPath,
        };
}
