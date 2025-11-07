using Fip.Strive.Core.Domain.Schemas.Queue.Enums;
using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;
using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Harvester.Domain.Signals;

public class TypedSignal() : Signal(SignalTypes.TypedSignal)
{
    public required string Filepath { get; set; }
    public required string Hash { get; set; }
    public required DataSources Source { get; set; }
    public required DateTime Timestamp { get; set; }
}
