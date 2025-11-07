using Fip.Strive.Core.Domain.Schemas.Queue.Enums;
using Fip.Strive.Core.Domain.Schemas.Queue.Models;
using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;
using Fip.Strive.Harvester.Application.Core.Hubs;
using Fip.Strive.Harvester.Application.Core.Queue.Components;
using Fip.Strive.Harvester.Domain.Signals;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Fip.Strive.Harvester.Application.Features.HelloWorld.Services;

public class HelloWorldWorker(ILogger<HelloWorldWorker> logger, IHubContext<HelloWorldHub> hub)
    : QueueWorker(SignalTypes.HelloWorldSignal)
{
    public override async Task ProcessAsync(JobDetails job, CancellationToken ct)
    {
        var signal = GetSafePayload<HelloWorldSignal>(job);

        logger.LogInformation("Hello world worker received a {DiceRoll} on a D20", signal.DiceRoll);

        await hub.Clients.All.SendAsync(
            "HelloWorld",
            $"Psst worker here, did you hear, the hello world job rolled a {signal.DiceRoll} on a D20."
        );
    }
}
