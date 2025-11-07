using Fip.Strive.Core.Domain.Schemas.Queue.Enums;
using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;

namespace Fip.Strive.Harvester.Domain.Signals;

public class HelloWorldSignal() : Signal(SignalTypes.HelloWorldSignal)
{
    public int DiceRoll { get; set; } = new Random().Next(1, 21);
}
