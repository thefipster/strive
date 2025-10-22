using System.Text.Json;
using Fip.Strive.Harvester.Application.Core.Queue.Enums;
using Fip.Strive.Harvester.Application.Core.Queue.Models;

namespace Fip.Strive.Harvester.Application.Features.Schedule.Signals;

public class HelloWorldSignal() : Signal(SignalTypes.HelloWorldSignal)
{
    public int DiceRoll { get; set; } = new Random().Next(1, 21);

    public override JobDetails ToJobEntity()
    {
        return new JobDetails
        {
            Id = Id,
            Type = Type,
            SignalledAt = EmittedAt,
            Payload = JsonSerializer.Serialize(this),
        };
    }
}
