using Fip.Strive.Harvester.Application.Core.PubSub.Contracts;
using Fip.Strive.Harvester.Domain.Defaults;
using Fip.Strive.Harvester.Domain.Signals;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Harvester.Pipeline.Assimilate.Cli.Application;

public class Worker(IPubSubClient client, IOptions<Config> config, ILogger<Worker> logger)
    : IProcessor
{
    private readonly DirectExchange _exchange = HarvestPipelineExchange.New(
        SignalTypes.TypedSignal
    );

    public async Task ProcessAsync(string inMessage, CancellationToken ct)
    {
        logger.LogDebug($"Worker received message: {inMessage}");

        var inSignal = TypedSignal.From(inMessage);

        // extract data

        // publish index
    }
}
