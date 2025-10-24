using System.Text.Json;
using Fip.Strive.Core.Domain.Schemas.Queue.Enums;

namespace Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;

public class HelloWorldSignal() : Signal(SignalTypes.HelloWorldSignal)
{
    public int DiceRoll { get; set; } = new Random().Next(1, 21);
}
