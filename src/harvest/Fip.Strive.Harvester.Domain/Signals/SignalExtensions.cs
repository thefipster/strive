namespace Fip.Strive.Harvester.Domain.Signals;

public static class SignalExtensions
{
    public static string ToSignalName(this SignalTypes signal)
    {
        var signalName = signal.ToString();
        signalName = signalName.ToLowerInvariant();
        signalName = signalName.Replace("signal", string.Empty);
        return signalName;
    }
}
