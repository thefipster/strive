using Fip.Strive.Harvester.Application.Core.Queue.Models;

namespace Fip.Strive.Harvester.Application.Core.Queue.Components.Contracts;

public interface IJobStorage
{
    void Insert(JobEntity job);
    IEnumerable<JobEntity> GetPending(int count);
    IEnumerable<JobEntity> GetCompleted(int count);
    void MarkAsStarted(Guid id);
    void MarkAsSuccess(Guid id);
    void MarkAsFailed(Guid id, string message);
    void MarkAsFailed(Guid id, string message, Exception ex);
    void Dispose();
}
