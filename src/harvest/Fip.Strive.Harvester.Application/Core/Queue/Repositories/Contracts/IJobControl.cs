using Fip.Strive.Core.Domain.Schemas.Queue.Models;
using Fip.Strive.Harvester.Application.Core.Queue.Models;

namespace Fip.Strive.Harvester.Application.Core.Queue.Repositories.Contracts;

public interface IJobControl : IDisposable
{
    void Insert(JobDetails job);
    IEnumerable<JobDetails> GetStored(int count);
    IEnumerable<JobDetails> GetCompleted(int count);
    void MarkAsStarted(Guid id);
    void MarkAsSuccess(Guid id);
    void MarkAsFailed(Guid id, string message);
    void MarkAsFailed(Guid id, string message, Exception ex);
    void Reset();
}
