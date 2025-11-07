using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;
using Fip.Strive.Harvester.Application.Core.Hubs;
using Fip.Strive.Harvester.Application.Core.Queue.Components.Contracts;
using Fip.Strive.Harvester.Application.Core.Schedule;
using Fip.Strive.Harvester.Domain.Signals;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Fip.Strive.Harvester.Application.Features.HelloWorld.Jobs;

[RegularIntervalJob(5)]
public class HelloWorldJob(
    ILogger<HelloWorldJob> logger,
    ISignalQueue queue,
    IHubContext<HelloWorldHub> hub
) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var signal = new HelloWorldSignal();

        logger.LogInformation(
            "Hello world job at {Time} rolled a {DiceRoll} on a D20.",
            DateTimeOffset.Now,
            signal.DiceRoll
        );

        await hub.Clients.All.SendAsync(
            "HelloWorld",
            $"Hi there, I rolled a {signal.DiceRoll} on a D20."
        );

        await queue.EnqueueAsync(signal);
    }
}
