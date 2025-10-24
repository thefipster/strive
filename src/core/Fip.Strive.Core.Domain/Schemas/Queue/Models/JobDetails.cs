using Fip.Strive.Core.Domain.Schemas.Queue.Enums;

namespace Fip.Strive.Core.Domain.Schemas.Queue.Models;

public class JobDetails
{
    public Guid Id { get; set; }
    public SignalTypes Type { get; set; }
    public string? Payload { get; set; }
    public string? Error { get; set; }
    public JobStatus Status { get; set; } = JobStatus.Stored;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public DateTime SignalledAt { get; set; }
}
