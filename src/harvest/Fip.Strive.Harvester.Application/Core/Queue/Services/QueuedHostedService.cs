using Fip.Strive.Harvester.Application.Core.Queue.Components.Contracts;
using Fip.Strive.Harvester.Application.Core.Queue.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Harvester.Application.Core.Queue.Services;

public class QueuedHostedService(
    IOptions<QueueConfig> config,
    ILogger<QueuedHostedService> logger,
    IQueueWorkerFactory workerFactory
) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken ct)
    {
        logger.LogInformation("Queued hosted service started");

        var metrics = new QueueMetrics(config);

        var workers = Enumerable
            .Range(0, config.Value.MaxDegreeOfParallelism)
            .Select(id => Task.Run(() => workerFactory.CreateRunner(metrics).RunAsync(id, ct), ct))
            .ToList();

        workers.Add(Task.Run(() => workerFactory.CreateReporter(metrics).RunAsync(ct), ct));

        Task.WhenAll(workers);

        logger.LogInformation("Queued hosted service finished");

        return Task.CompletedTask;
    }
}
