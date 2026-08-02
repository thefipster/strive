using Fip.Strive.Harvester.Application.Core.Signals;

namespace Fip.Strive.Harvester.Application.Defaults;

public abstract class DirectExchange
{
    public abstract string Exchange { get; }
    public abstract string Route { get; }
    public abstract string Queue { get; }
    public abstract Dictionary<string, object?>? Attributes { get; }
}

public class HarvestPipelineExchange : DirectExchange
{
    public static DirectExchange Quarantine =>
        new HarvestPipelineExchange("quarantine", GetTtlAttributes(TimeSpan.FromDays(30)));

    public static IEnumerable<DirectExchange> All =>
        [
            New(SignalTypes.UploadSignal),
            New(SignalTypes.ImportSignal),
            New(SignalTypes.ScanSignal),
            New(SignalTypes.FileSignal),
            New(SignalTypes.TypedSignal),
        ];

    private HarvestPipelineExchange(string name, Dictionary<string, object?>? attributes = null)
    {
        Exchange = $"strive.harvest.{name}.exchange";
        Queue = $"strive.harvest.{name}.queue";
        Route = $"strive.harvest.{name}.route";
        Attributes = attributes;
    }

    public static DirectExchange New(SignalTypes signal)
    {
        if (signal == SignalTypes.VoidSignal)
            throw new InvalidOperationException("Can't create queue from VoidSignal.");

        var name = signal.ToSignalName();
        return new HarvestPipelineExchange(name);
    }

    private static Dictionary<string, object?> GetTtlAttributes(TimeSpan ttl) =>
        new() { { "x-message-ttl", (int)ttl.TotalMilliseconds } };

    public override string Exchange { get; }
    public override string Route { get; }
    public override string Queue { get; }
    public override Dictionary<string, object?>? Attributes { get; }
}
