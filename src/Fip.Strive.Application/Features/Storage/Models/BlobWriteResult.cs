namespace Fip.Strive.Application.Features.Storage.Models;

/// <param name="Hash">Lowercase hex SHA-256 of the content — the blob's identity.</param>
/// <param name="SizeBytes">Number of bytes read from the source stream.</param>
/// <param name="WasWritten">
/// False when an identical blob was already on disk and the write was discarded. This is
/// dedup-on-write observed at the filesystem, not a statement about the catalog: a blob can exist
/// on disk without a catalog row if a previous import was interrupted.
/// </param>
public readonly record struct BlobWriteResult(string Hash, long SizeBytes, bool WasWritten);
