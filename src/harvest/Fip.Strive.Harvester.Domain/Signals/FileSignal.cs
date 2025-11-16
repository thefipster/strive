using System.Text.Json;

namespace Fip.Strive.Harvester.Domain.Signals;

public class FileSignal() : EnumSignal(SignalTypes.FileSignal)
{
    public required string Filepath { get; set; }
    public required string Hash { get; set; }
    public required string ParentFilepath { get; set; }

    public string ToJson() => JsonSerializer.Serialize(this);

    public static FileSignal From(string filepath, string hash, ScanSignal signal) =>
        new FileSignal
        {
            Filepath = filepath,
            Hash = hash,
            ParentFilepath = signal.ZipPath,
            ReferenceId = signal.ReferenceId,
        };

    public static FileSignal From(string inMessage) =>
        JsonSerializer.Deserialize<FileSignal>(inMessage)
        ?? throw new InvalidOperationException("Invalid message");
}
