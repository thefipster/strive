using Fip.Strive.Application.Features.Jobs.Models;

namespace Fip.Strive.Application.Features.Jobs.Services.Contracts;

public interface IJobReader
{
    Task<JobCounts> GetCountsAsync(CancellationToken cancellationToken = default);

    /// <summary>Unfinished work first, then the most recent finished work.</summary>
    Task<IReadOnlyList<JobRow>> GetJobsAsync(
        int take = 100,
        CancellationToken cancellationToken = default
    );
}
