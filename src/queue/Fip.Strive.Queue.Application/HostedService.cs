using Fip.Strive.Queue.Application.Services.Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Fip.Strive.Queue.Application;

public class HostedService(IProcessingService processor, ILogger<HostedService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        logger.LogInformation("Queued hosted service started");

        await processor.ExecuteAsync(ct);

        logger.LogInformation("Queued hosted service finished");
    }
}
