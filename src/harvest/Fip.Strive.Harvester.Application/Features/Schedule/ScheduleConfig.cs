namespace Fip.Strive.Harvester.Application.Features.Schedule;

public class ScheduleConfig
{
    public const string ConfigSectionName = "Features:Schedule";

    public int HelloWorldIntervalSeconds { get; set; } = 5;
    public int QueueCleanupIntervalSeconds { get; set; } = 86400;
}
