namespace Fip.Strive.Harvester.Application.Core.Signals;

public class HelloWorldSignal() : EnumSignal(SignalTypes.HelloWorldSignal)
{
    public int DiceRoll { get; set; } = new Random().Next(1, 21);
}
