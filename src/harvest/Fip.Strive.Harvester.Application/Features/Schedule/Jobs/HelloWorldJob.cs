using Fip.Strive.Harvester.Application.Core.Queue.Components.Contracts;
using Fip.Strive.Harvester.Application.Features.Schedule.Signals;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Fip.Strive.Harvester.Application.Features.Schedule.Jobs;

public class HelloWorldJob(ILogger<HelloWorldJob> logger, ISignalQueue queue) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("Hello world at {Time}", DateTimeOffset.Now);

        var signal = new HelloWorldSignal();
        await queue.EnqueueAsync(signal);
    }
}
