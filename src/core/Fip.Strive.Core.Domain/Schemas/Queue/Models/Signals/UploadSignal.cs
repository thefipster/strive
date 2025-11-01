using Fip.Strive.Core.Domain.Schemas.Queue.Enums;

namespace Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;

public class UploadSignal() : Signal(SignalTypes.UploadSignal)
{
    public required string Filepath { get; set; }
    public required string Hash { get; set; }

    public static UploadSignal From(string filepath, string hash) =>
        new UploadSignal { Filepath = filepath, Hash = hash };
}
