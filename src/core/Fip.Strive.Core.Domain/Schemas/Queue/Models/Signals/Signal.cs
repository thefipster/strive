using System.Text.Json;
using Fip.Strive.Core.Domain.Schemas.Queue.Enums;

namespace Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;

public class Signal(SignalTypes type)
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime EmittedAt { get; set; } = DateTime.UtcNow;
    public SignalTypes Type { get; set; } = type;
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
