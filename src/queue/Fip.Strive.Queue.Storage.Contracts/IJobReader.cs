using Fip.Strive.Core.Domain.Schemas.Requests.Paging;
using Fip.Strive.Queue.Domain.Enums;
using Fip.Strive.Queue.Domain.Models;

namespace Fip.Strive.Queue.Storage.Contracts;

public interface IJobReader
{
    PagedResponse<JobDetails> GetUpcommingJobs(int page, int size);

    PagedResponse<JobDetails> GetSucceededJobs(int page, int size);

    PagedResponse<JobDetails> GetFailedJobs(int page, int size);

    PagedResponse<JobDetails> GetJobs(JobStatus status, int page, int size);

    PagedResponse<JobDetails> GetJobs(int page, int size, params JobStatus[] statuses);
}
