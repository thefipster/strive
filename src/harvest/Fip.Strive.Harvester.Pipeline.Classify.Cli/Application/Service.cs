using Fip.Strive.Harvester.Application.Core.PubSub.Contracts;
using Fip.Strive.Harvester.Domain.Defaults;
using Fip.Strive.Harvester.Domain.Signals;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Fip.Strive.Harvester.Pipeline.Classify.Cli.Application;

public class Service(IPubSubClient client, IProcessor processor, ILogger<Service> logger)
    : BackgroundService
{
    private readonly DirectExchange _exchange = HarvestPipelineExchange.New(SignalTypes.FileSignal);
    private readonly DirectExchange _quarantine = HarvestPipelineExchange.Quarantine;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        logger.LogInformation("Harvester Pipeline Classify started.");

        await client.SubscribeAsync(_exchange, _quarantine, processor.ProcessAsync, ct);
        await Task.Delay(Timeout.InfiniteTimeSpan, ct);

        logger.LogInformation("Harvester Pipeline Classify finished.");
    }
}
