using System.Text.Json;
using Fip.Strive.Harvester.Application.Core.Queue.Enums;

namespace Fip.Strive.Harvester.Application.Core.Queue.Models;

public class Signal(SignalTypes type)
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime EmittedAt { get; set; } = DateTime.UtcNow;
    public SignalTypes Type { get; set; } = type;

    public virtual JobDetails ToJobEntity()
    {
        return new JobDetails
        {
            Id = Id,
            Type = Type,
            SignalledAt = EmittedAt,
            Payload = JsonSerializer.Serialize(this),
        };
    }
}
