using Fip.Strive.Core.Application.Features.Config.Models;

namespace Fip.Strive.Harvester.Application.Core.Schedule;

public class ScheduleConfig() : BaseConfig("Features:Schedule")
{
    public int HelloWorldIntervalSeconds { get; set; } = 5;
    public int QueueCleanupIntervalSeconds { get; set; } = 86400;
}
