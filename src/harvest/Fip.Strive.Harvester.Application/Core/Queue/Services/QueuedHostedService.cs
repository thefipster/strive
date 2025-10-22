using Fip.Strive.Harvester.Application.Core.Hubs;
using Fip.Strive.Harvester.Application.Core.Queue.Components;
using Fip.Strive.Harvester.Application.Core.Queue.Components.Contracts;
using Fip.Strive.Harvester.Application.Core.Queue.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Harvester.Application.Core.Queue.Services;

public class QueuedHostedService(
    IOptions<QueueConfig> config,
    ISignalQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<QueuedHostedService> logger,
    ILoggerFactory loggerFactory,
    IHubContext<QueueHub> hub
) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken ct)
    {
        logger.LogInformation("Queued hosted service started");

        var metrics = new QueueMetrics(config);

        var workers = Enumerable
            .Range(0, config.Value.MaxDegreeOfParallelism)
            .Select(id =>
                Task.Run(
                    () =>
                        new QueueRunner(
                            queue,
                            scopeFactory,
                            loggerFactory.CreateLogger<QueueRunner>(),
                            metrics,
                            config
                        ).RunAsync(id, ct),
                    ct
                )
            )
            .ToList();

        workers.Add(
            Task.Run(
                () =>
                    new QueueReporter(
                        queue,
                        loggerFactory.CreateLogger<QueueReporter>(),
                        config,
                        hub,
                        metrics
                    ).RunAsync(ct),
                ct
            )
        );

        Task.WhenAll(workers);

        logger.LogInformation("Queued hosted service finished");

        return Task.CompletedTask;
    }
}
