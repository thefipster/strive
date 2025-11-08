using Fip.Strive.Queue.Application.Services.Contracts;
using Fip.Strive.Queue.Application.Tasks.Contracts;
using Fip.Strive.Queue.Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Queue.Application.Tasks;

public class QueueReporter(
    IQueueService queue,
    ILogger<QueueReporter> logger,
    IOptions<QueueConfig> config,
    QueueMetrics metrics
) : IQueueReporter
{
    private readonly TimeSpan _updateDelay = TimeSpan.FromMilliseconds(config.Value.UpdateDelayMs);

    private bool _isRunning;

    public bool IsRunning => _isRunning;

    public async Task RunAsync(CancellationToken ct)
    {
        logger.LogInformation("QueueReporter starting.");
        _isRunning = true;

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
        _isRunning = false;
    }

    private Task ReportAsync()
    {
        logger.LogTrace(
            $"Queue is processing - Jobs: {queue.Count}, Runners: {metrics.ActiveWorkers}, Rate: {metrics.GetProcessingRate()}"
        );

        return Task.CompletedTask;
    }

    private Task ReportFinishedAsync()
    {
        logger.LogInformation("Queue has finished processing.");
        return Task.CompletedTask;
    }
}
