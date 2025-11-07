namespace Fip.Strive.Harvester.Domain.Signals;

public class UploadSignal() : EnumSignal(SignalTypes.UploadSignal)
{
    public required string Filepath { get; set; }
    public required string Hash { get; set; }

    public static UploadSignal From(string filepath, string hash) =>
        new UploadSignal { Filepath = filepath, Hash = hash };
}
