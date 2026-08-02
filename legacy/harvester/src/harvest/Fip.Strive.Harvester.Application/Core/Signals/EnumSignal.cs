namespace Fip.Strive.Harvester.Application.Core.Signals;

public class EnumSignal(SignalTypes type) : Signal((int)type);
