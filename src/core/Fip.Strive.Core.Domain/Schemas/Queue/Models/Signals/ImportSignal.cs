using System.Text.Json;
using Fip.Strive.Core.Domain.Schemas.Queue.Enums;

namespace Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;

public class ImportSignal() : Signal(SignalTypes.ImportSignal)
{
    public required string Filepath { get; set; }
    public required string Hash { get; set; }

    public static ImportSignal From(string filepath, UploadSignal originSignal)
    {
        return new ImportSignal
        {
            Filepath = filepath,
            Hash = originSignal.Hash,
            ReferenceId = originSignal.ReferenceId,
        };
    }
}
