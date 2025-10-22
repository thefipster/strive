using Fip.Strive.Harvester.Application.Core.Hubs;
using Fip.Strive.Harvester.Application.Core.Queue.Components.Contracts;
using Fip.Strive.Harvester.Application.Core.Queue.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Harvester.Application.Core.Queue.Components;

public class QueueReporter(
    ISignalQueue queue,
    ILogger<QueueReporter> logger,
    IOptions<QueueConfig> config,
    IHubContext<QueueHub> hub,
    QueueMetrics metrics
) : IQueueReporter
{
    private readonly TimeSpan _updateDelay = TimeSpan.FromMilliseconds(config.Value.UpdateDelayMs);

    public async Task RunAsync(CancellationToken ct)
    {
        logger.LogInformation("QueueReporter starting.");

        int lastCount = 0;
        bool finalized = false;

        while (!ct.IsCancellationRequested)
        {
            try
            {
                var count = queue.Count;
                var rate = metrics.GetProcessingRate();
                if (count != 0 || lastCount != 0 || rate > 0.1)
                {
                    await ReportAsync();
                    finalized = false;
                }
                else if (!finalized)
                {
                    await ReportFinishedAsync();
                    finalized = true;
                }

                lastCount = count;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in reporter loop.");
            }
            finally
            {
                await Task.Delay(_updateDelay, ct);
            }
        }

        logger.LogInformation("Reporter stopping.");
    }

    private async Task ReportAsync()
    {
        logger.LogTrace(
            $"Queue is processing - Jobs: {queue.Count}, Runners: {metrics.ActiveWorkers}, Rate: {metrics.GetProcessingRate()}"
        );

        await hub.Clients.All.SendAsync(
            QueueHub.QueueReportMethodName,
            queue.Count,
            metrics.ActiveWorkers,
            metrics.GetProcessingRate()
        );
    }

    private async Task ReportFinishedAsync()
    {
        logger.LogInformation("Queue has finished processing.");
        await hub.Clients.All.SendAsync(QueueHub.QueueReportMethodName, 0, 0, 0);
    }
}
