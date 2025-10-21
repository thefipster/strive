using Fip.Strive.Harvester.Application.Core.Queue.Components.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Harvester.Application.Core.Queue.Components;

public class QueueReporter(
    ISignalQueue queue,
    ILogger<QueueReporter> logger,
    IOptions<QueueConfig> config
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
                if (count != 0 || lastCount != 0)
                {
                    finalized = false;
                }
                else if (!finalized)
                {
                    OnQueueWorkFinished();
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

    private void OnQueueWorkFinished()
    {
        logger.LogInformation("Queue has finished processing.");
    }
}
