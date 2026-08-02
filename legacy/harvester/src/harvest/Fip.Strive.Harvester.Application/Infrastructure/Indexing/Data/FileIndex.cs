using Fip.Strive.Harvester.Application.Core.Signals;

namespace Fip.Strive.Harvester.Application.Infrastructure.Indexing.Data;

public class FileIndex
{
    public required string Filepath { get; set; }
    public required string ParentFilepath { get; set; }
    public required string Hash { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public static FileIndex From(FileSignal inSignal) =>
        new FileIndex
        {
            Filepath = inSignal.Filepath,
            ParentFilepath = inSignal.ParentFilepath,
            Hash = inSignal.Hash,
        };
}
