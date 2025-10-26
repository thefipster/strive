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
    private List<IQueueRunner> _runners = [];
    private List<Task> _workers = [];

    public bool IsRunning => _runners.Any(x => x.IsRunning);

    protected override Task ExecuteAsync(CancellationToken ct)
    {
        logger.LogInformation("Queued hosted service started");

        var metrics = new QueueMetrics(config);

        for (int i = 0; i < config.Value.MaxDegreeOfParallelism; i++)
        {
            var runner = workerFactory.CreateRunner(metrics);
            _runners.Add(runner);
            _workers.Add(runner.RunAsync(i, ct));
        }

        _workers.Add(Task.Run(() => workerFactory.CreateReporter(metrics).RunAsync(ct), ct));

        Task.WhenAll(_workers);

        logger.LogInformation("Queued hosted service finished");
        return Task.CompletedTask;
    }
}
