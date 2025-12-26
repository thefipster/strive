namespace Fip.Strive.Harvester.Application.Core.Signals;

public class Signal(int type)
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime EmittedAt { get; set; } = DateTime.UtcNow;
    public int Type { get; set; } = type;
    public Guid ReferenceId { get; set; } = Guid.NewGuid();
}
