using System.Text.Json;

namespace Fip.Strive.Harvester.Application.Core.Signals;

public class ImportSignal() : EnumSignal(SignalTypes.ImportSignal)
{
    public required string Filepath { get; set; }
    public required string Hash { get; set; }

    public static ImportSignal From(string filepath, UploadSignal originSignal) =>
        new ImportSignal
        {
            Filepath = filepath,
            Hash = originSignal.Hash,
            ReferenceId = originSignal.ReferenceId,
        };

    public static ImportSignal FromMessage(string message) =>
        JsonSerializer.Deserialize<ImportSignal>(message)
        ?? throw new InvalidOperationException("Invalid message");
}
