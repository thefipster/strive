using Fip.Strive.Harvester.Domain.Defaults;
using Fip.Strive.Harvester.Pipeline.ZipGate.Cli.Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Fip.Strive.Harvester.Pipeline.ZipGate.Cli;

public class Service(IPubSubClient client, IMessageProcessor processor, ILogger<Service> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        logger.LogInformation("Harvester Pipeline ZipGate started.");

        await client.SubscribeAsync(new UploadExchange(), processor.ProcessAsync, ct);
        await Task.Delay(Timeout.InfiniteTimeSpan, ct);

        logger.LogInformation("Harvester Pipeline ZipGate finished.");
    }
}
