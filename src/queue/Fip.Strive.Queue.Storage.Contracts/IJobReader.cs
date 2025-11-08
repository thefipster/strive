using Fip.Strive.Core.Domain.Schemas.Requests.Paging;
using Fip.Strive.Queue.Domain.Enums;
using Fip.Strive.Queue.Domain.Models;

namespace Fip.Strive.Queue.Storage.Contracts;

public interface IJobReader
{
    PagedResponse<JobDetails> GetJobs(int page, int size, params JobStatus[] statuses);
}
