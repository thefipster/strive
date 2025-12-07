namespace Fip.Strive.Harvester.Application.Infrastructure.Pipeline.Data;

public class QuarantinedSignal
{
    private QuarantinedSignal() { }

    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public required string Payload { get; set; }

    public static QuarantinedSignal From(string payload) => new() { Payload = payload };
}
