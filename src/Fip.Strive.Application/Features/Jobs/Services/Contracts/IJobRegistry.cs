using Fip.Strive.Application.Features.Jobs.Models;

namespace Fip.Strive.Application.Features.Jobs.Services.Contracts;

public interface IJobRegistry
{
    JobComponent Resolve(string kind);

    /// <summary>
    /// Every registered component. Step 3's invalidation sweep reads this to compare declared
    /// versions against the ones stamped on existing rows.
    /// </summary>
    IReadOnlyCollection<JobComponent> All { get; }
}
