using Fip.Strive.Harvester.Application.Core.Queue.Enums;

namespace Fip.Strive.Harvester.Application.Core.Queue.Models;

public class JobEntity
{
    public Guid Id { get; set; }
    public SignalTypes Type { get; set; }
    public string? Payload { get; set; }
    public string? Error { get; set; }
    public JobStatus Status { get; set; } = JobStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public DateTime SignalledAt { get; set; }
}
