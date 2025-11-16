using Fip.Strive.Harvester.Domain.Signals;

namespace Fip.Strive.Harvester.Domain.Defaults;

public abstract class DirectExchange
{
    public abstract string Exchange { get; }
    public abstract string Route { get; }
    public abstract string Queue { get; }
}

public class HarvestPipelineExchange : DirectExchange
{
    private HarvestPipelineExchange(string name)
    {
        Exchange = $"strive.harvest.{name}.exchange";
        Queue = $"strive.harvest.{name}.queue";
        Route = $"strive.harvest.{name}.route";
    }

    public static DirectExchange New(SignalTypes signal)
    {
        return signal switch
        {
            SignalTypes.UploadSignal => new HarvestPipelineExchange("upload"),
            SignalTypes.ImportSignal => new HarvestPipelineExchange("import"),
            SignalTypes.ScanSignal => new HarvestPipelineExchange("scanner"),
            SignalTypes.FileSignal => new HarvestPipelineExchange("files"),
            SignalTypes.TypedSignal => new HarvestPipelineExchange("source"),
            _ => throw new NotSupportedException(
                $"Signal type {signal} is not supported in harvest pipeline."
            ),
        };
    }

    public static IEnumerable<DirectExchange> All =>
        [
            New(SignalTypes.UploadSignal),
            New(SignalTypes.ImportSignal),
            New(SignalTypes.ScanSignal),
            New(SignalTypes.FileSignal),
            New(SignalTypes.TypedSignal),
        ];

    public override string Exchange { get; }
    public override string Route { get; }
    public override string Queue { get; }
}
