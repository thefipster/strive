using System.Text.Json;

namespace Fip.Strive.Harvester.Application.Core.Signals;

public class ScanSignal() : EnumSignal(SignalTypes.ScanSignal)
{
    public required string ZipPath { get; set; }
    public required string UnzipPath { get; set; }
    public required string Hash { get; set; }

    public static ScanSignal From(string unzipPath, ImportSignal inSignal) =>
        new ScanSignal
        {
            UnzipPath = unzipPath,
            ZipPath = inSignal.Filepath,
            Hash = inSignal.Hash,
            ReferenceId = inSignal.ReferenceId,
        };

    public static ScanSignal From(string message) =>
        JsonSerializer.Deserialize<ScanSignal>(message)
        ?? throw new InvalidOperationException("Invalid message");

    public string ToJson() => JsonSerializer.Serialize(this);
}
