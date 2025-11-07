namespace Fip.Strive.Harvester.Domain.Signals;

public class HelloWorldSignal() : EnumSignal(SignalTypes.HelloWorldSignal)
{
    public int DiceRoll { get; set; } = new Random().Next(1, 21);
}
