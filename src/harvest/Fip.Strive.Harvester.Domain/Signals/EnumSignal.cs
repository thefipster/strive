namespace Fip.Strive.Harvester.Domain.Signals;

public class EnumSignal(SignalTypes type) : Signal((int)type);
