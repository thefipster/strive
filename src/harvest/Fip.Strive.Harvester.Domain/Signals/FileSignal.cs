namespace Fip.Strive.Harvester.Domain.Signals;

public class FileSignal() : EnumSignal(SignalTypes.FileSignal)
{
    public required string Filepath { get; set; }
    public required string Hash { get; set; }
}
