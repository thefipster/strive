using Fip.Strive.Core.Domain.Schemas.Queue.Enums;
using Fip.Strive.Core.Domain.Schemas.Queue.Models;
using Fip.Strive.Harvester.Application.Core.Queue.Models;
using Fip.Strive.Harvester.Application.Infrastructure.Models;

namespace Fip.Strive.Harvester.Application.Core.Queue.Repositories.Contracts;

public interface IJobReader
{
    public PagedResponse<JobDetails> GetUpcommingJobs(int page, int size);

    public PagedResponse<JobDetails> GetSucceededJobs(int page, int size);

    public PagedResponse<JobDetails> GetFailedJobs(int page, int size);

    public PagedResponse<JobDetails> GetJobs(JobStatus status, int page, int size);
}
