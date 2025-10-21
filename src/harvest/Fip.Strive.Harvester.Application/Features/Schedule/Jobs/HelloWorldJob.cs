using Fip.Strive.Harvester.Application.Core.Queue.Components.Contracts;
using Fip.Strive.Harvester.Application.Features.Schedule.Signals;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Fip.Strive.Harvester.Application.Features.Schedule.Jobs;

public class HelloWorldJob(ILogger<HelloWorldJob> logger, ISignalQueue queue) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var signal = new HelloWorldSignal();

        logger.LogInformation(
            "Hello world job at {Time} rolled a {DiceRoll} on a D20.",
            DateTimeOffset.Now,
            signal.DiceRoll
        );

        await queue.EnqueueAsync(signal);
    }
}
