using System.Text.Json;
using Fip.Strive.Harvester.Application.Core.Queue.Components.Contracts;
using Fip.Strive.Harvester.Application.Core.Queue.Enums;
using Fip.Strive.Harvester.Application.Core.Queue.Exceptions;
using Fip.Strive.Harvester.Application.Core.Queue.Models;
using Fip.Strive.Harvester.Application.Features.Schedule.Signals;
using Microsoft.Extensions.Logging;

namespace Fip.Strive.Harvester.Application.Features.Schedule.Workers;

public class HelloWorldWorker(ILogger<HelloWorldWorker> logger) : ISignalQueueWorker
{
    public SignalTypes Type => SignalTypes.HelloWorldSignal;

    public Task ProcessAsync(JobEntity job, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(job.Payload))
            throw new InvalidJobException(job, "Payload is null or empty.");

        var signal = JsonSerializer.Deserialize<HelloWorldSignal>(job.Payload);
        if (signal == null)
            throw new InvalidJobException(job, "Can't read payload.");

        logger.LogInformation("Hello world rolled a {DiceRoll} on a D20", signal.DiceRoll);
        return Task.CompletedTask;
    }
}
