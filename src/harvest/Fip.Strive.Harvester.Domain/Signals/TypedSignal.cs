using System.Text.Json;
using Fip.Strive.Harvester.Domain.Indexes;
using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Harvester.Domain.Signals;

public class TypedSignal() : EnumSignal(SignalTypes.TypedSignal)
{
    public required string Filepath { get; set; }
    public required string Hash { get; set; }
    public required DataSources Source { get; set; }

    [Obsolete("Classification no longer provides time data since its useless.")]
    public DateTime Timestamp { get; set; }

    public string ToJson() => JsonSerializer.Serialize(this);

    public static TypedSignal From(SourceIndex source, FileSignal inSignal) =>
        new TypedSignal
        {
            Filepath = inSignal.Filepath,
            Hash = inSignal.Hash,
            Source = source.Source,
        };

    public static TypedSignal From(string message) =>
        JsonSerializer.Deserialize<TypedSignal>(message)
        ?? throw new InvalidOperationException("Invalid message");
}
