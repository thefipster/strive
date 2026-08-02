namespace Fip.Strive.Application.Features.Import.Models;

public enum ImportOutcome
{
    /// <summary>The archive was unpacked and catalogued.</summary>
    Imported,

    /// <summary>An archive with identical bytes was already imported. No work was done.</summary>
    DuplicateArchive,
}

/// <param name="PackageId">
/// The package created by this import, or — for <see cref="ImportOutcome.DuplicateArchive"/> — the
/// one that already held these bytes.
/// </param>
/// <param name="FileCount">Files in the archive's manifest.</param>
/// <param name="NewEntryCount">How many of them were content the catalog had never seen.</param>
public sealed record ImportResult(
    ImportOutcome Outcome,
    Guid PackageId,
    int FileCount,
    int NewEntryCount
);
