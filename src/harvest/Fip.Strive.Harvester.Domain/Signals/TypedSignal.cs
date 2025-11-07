using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Harvester.Domain.Signals;

public class TypedSignal() : EnumSignal(SignalTypes.TypedSignal)
{
    public required string Filepath { get; set; }
    public required string Hash { get; set; }
    public required DataSources Source { get; set; }
    public required DateTime Timestamp { get; set; }
}
