using Fip.Strive.Core.Domain.Schemas.Queue.Enums;
using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;

namespace Fip.Strive.Harvester.Domain.Signals;

public class FileSignal() : Signal(SignalTypes.FileSignal)
{
    public required string Filepath { get; set; }
    public required string Hash { get; set; }
}
