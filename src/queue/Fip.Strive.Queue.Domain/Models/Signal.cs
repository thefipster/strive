using System.Text.Json;
using Fip.Strive.Queue.Domain.Enums;

namespace Fip.Strive.Queue.Domain.Models;

public class Signal(int type)
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime EmittedAt { get; set; } = DateTime.UtcNow;
    public int Type { get; set; } = type;
    public Guid ReferenceId { get; set; } = Guid.NewGuid();

    public JobDetails ToJobEntity()
    {
        return new JobDetails
        {
            Id = Id,
            Type = Type,
            SignalledAt = EmittedAt,
            Payload = JsonSerializer.Serialize(this, GetType()),
        };
    }
}
