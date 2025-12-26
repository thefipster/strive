using System.Text.Json;

namespace Fip.Strive.Harvester.Application.Core.Signals;

public class UploadSignal() : EnumSignal(SignalTypes.UploadSignal)
{
    public required string Filepath { get; set; }
    public required string Hash { get; set; }

    public string ToJson() => JsonSerializer.Serialize(this);

    public static UploadSignal From(string filepath, string hash) =>
        new UploadSignal { Filepath = filepath, Hash = hash };

    public static UploadSignal FromMessage(string message) =>
        JsonSerializer.Deserialize<UploadSignal>(message)
        ?? throw new InvalidOperationException("Invalid message");
}
