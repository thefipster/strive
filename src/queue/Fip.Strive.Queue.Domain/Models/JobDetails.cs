using Fip.Strive.Queue.Domain.Enums;

namespace Fip.Strive.Queue.Domain.Models;

public class JobDetails
{
    public Guid Id { get; set; }
    public int Type { get; set; }
    public string? Payload { get; set; }
    public string? Error { get; set; }
    public JobStatus Status { get; set; } = JobStatus.Stored;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public DateTime SignalledAt { get; set; }
}
