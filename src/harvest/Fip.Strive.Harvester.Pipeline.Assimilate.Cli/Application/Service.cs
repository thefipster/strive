using Fip.Strive.Harvester.Application.Core.PubSub.Contracts;
using Fip.Strive.Harvester.Domain.Defaults;
using Fip.Strive.Harvester.Domain.Signals;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Fip.Strive.Harvester.Pipeline.Assimilate.Cli.Application;

public class Service(IPubSubClient client, IProcessor processor, ILogger<Service> logger)
    : BackgroundService
{
    private readonly DirectExchange _exchange = HarvestPipelineExchange.New(
        SignalTypes.TypedSignal
    );

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        logger.LogInformation("Harvester Pipeline Assimilate started.");

        await client.SubscribeAsync(_exchange, processor.ProcessAsync, ct);
        await Task.Delay(Timeout.InfiniteTimeSpan, ct);

        logger.LogInformation("Harvester Pipeline Assimilate finished.");
    }
}
