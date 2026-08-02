using Fip.Strive.Application.Features.Import.Models;
using Fip.Strive.Application.Features.Storage.Models;

namespace Fip.Strive.Application.Features.Import.Services.Contracts;

public interface IPackageImporter
{
    /// <summary>
    /// Unpacks a staged archive into the blob store and records its manifest. Re-importing an
    /// archive that was already imported does no work and reports the original package.
    /// </summary>
    Task<ImportResult> ImportAsync(
        StagedArchive archive,
        IProgress<ImportProgress>? progress = null,
        CancellationToken cancellationToken = default
    );
}
