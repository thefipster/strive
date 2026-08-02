namespace Fip.Strive.Harvester.Application.Core.Schedule;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class RegularIntervalJobAttribute(int seconds) : Attribute
{
    public TimeSpan Interval { get; } = TimeSpan.FromSeconds(seconds);
}
