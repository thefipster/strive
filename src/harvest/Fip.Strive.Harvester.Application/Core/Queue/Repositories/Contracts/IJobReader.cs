using Fip.Strive.Core.Domain.Schemas.Queue.Enums;
using Fip.Strive.Core.Domain.Schemas.Queue.Models;
using Fip.Strive.Core.Domain.Schemas.Requests.Paging;

namespace Fip.Strive.Harvester.Application.Core.Queue.Repositories.Contracts;

public interface IJobReader
{
    PagedResponse<JobDetails> GetUpcommingJobs(int page, int size);

    PagedResponse<JobDetails> GetSucceededJobs(int page, int size);

    PagedResponse<JobDetails> GetFailedJobs(int page, int size);

    PagedResponse<JobDetails> GetJobs(JobStatus status, int page, int size);

    PagedResponse<JobDetails> GetJobs(int page, int size, params JobStatus[] statuses);
}
