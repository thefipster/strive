using Fip.Strive.Queue.Domain.Models;

namespace Fip.Strive.Queue.Storage.Memory.Contexts;

public class MemoryQueueContext
{
    public HashSet<JobDetails> Jobs { get; set; } = [];
}
