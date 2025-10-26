using System.Text.Json;
using Fip.Strive.Core.Domain.Schemas.Queue.Enums;

namespace Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;

public class FileSignal() : Signal(SignalTypes.FileSignal)
{
    public required string Filepath { get; set; }
}
