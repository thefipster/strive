namespace Fip.Strive.Application.Features.Storage.Models;

/// <summary>
/// An uploaded archive sitting in the staging area, hashed but not yet imported.
/// </summary>
/// <param name="FileName">Original name as uploaded, kept for display only.</param>
/// <param name="Path">Absolute path in the staging area.</param>
/// <param name="Hash">Lowercase hex SHA-256 of the archive bytes — the package's identity.</param>
public sealed record StagedArchive(string FileName, string Path, string Hash, long SizeBytes);
