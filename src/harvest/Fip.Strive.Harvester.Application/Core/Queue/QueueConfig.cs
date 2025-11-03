using Fip.Strive.Core.Application.Features.Config.Models;

namespace Fip.Strive.Harvester.Application.Core.Queue;

public class QueueConfig() : BaseConfig("Features:Queue")
{
    public int QueueCountLimit { get; set; } = 1000;
    public int QueueBatchSize { get; set; } = 500;
    public string DatabasePath { get; set; } = "data/db/jobs.db";
    public int MaxDegreeOfParallelism { get; set; } = 1;
    public long RateWindowSeconds { get; set; } = 5;
    public long ProcessingDelayMs { get; set; } = 10;
    public long UpdateDelayMs { get; set; } = 100;
    public int DeleteAfterDays { get; set; } = 30;
}
