using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Domain.Schemas.Queue.Enums;

namespace Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;

public class TypedSignal() : Signal(SignalTypes.TypedSignal)
{
    public required string Filepath { get; set; }
    public required string Hash { get; set; }
    public required DataSources Source { get; set; }
    public required DateTime Timestamp { get; set; }
}
