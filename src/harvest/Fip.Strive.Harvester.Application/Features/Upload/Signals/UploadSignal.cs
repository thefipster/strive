using Fip.Strive.Harvester.Application.Core.Queue.Enums;
using Fip.Strive.Harvester.Application.Core.Queue.Models;

namespace Fip.Strive.Harvester.Application.Features.Upload.Signals;

public class UploadSignal() : Signal(SignalTypes.UploadSignal)
{
    public required string Filepath { get; set; }
}
