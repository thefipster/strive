using System.IO.Compression;
using Fip.Strive.Application.Features.Storage;

namespace Fip.Strive.Application.Features.Import.Services;

/// <summary>
/// Bounds what an archive is allowed to expand into. <c>MaxUploadBytes</c> caps the *compressed*
/// upload, which says nothing about what it unpacks to — a megabyte of zeros compresses to a few
/// hundred bytes, so a small archive can fill a disk. The upload page that accepts these is
/// unauthenticated by design (see the Readme's LAN-only note), which makes the ceiling worth having
/// even in a homelab.
/// </summary>
/// <remarks>
/// Checked twice on purpose. <see cref="ZipArchiveEntry.Length"/> is the size the archive *claims*,
/// which is cheap to read and enough to reject an obvious bomb before writing a byte — but it comes
/// from the file being validated, so a hostile archive can simply lie. The stream returned by
/// <see cref="Bound"/> therefore counts what is actually read and aborts mid-entry when the claim
/// turns out to be false.
/// </remarks>
public sealed class ExpansionGuard(StorageOptions limits)
{
    private long _written;

    /// <summary>
    /// Pre-flight against what the archive declares about itself. Runs before anything is written.
    /// </summary>
    public void CheckDeclared(IReadOnlyList<ZipArchiveEntry> entries)
    {
        if (entries.Count > limits.MaxArchiveEntries)
            throw new InvalidDataException(
                $"The archive holds {entries.Count:N0} files, above the "
                    + $"{limits.MaxArchiveEntries:N0} this instance accepts. Raise "
                    + $"{StorageOptions.SectionName}:{nameof(StorageOptions.MaxArchiveEntries)} if that is genuinely expected."
            );

        long declaredTotal = 0;

        foreach (var entry in entries)
        {
            if (entry.Length > limits.MaxEntryBytes)
                throw new InvalidDataException(
                    $"'{entry.FullName}' unpacks to {entry.Length:N0} bytes, above the "
                        + $"{limits.MaxEntryBytes:N0} allowed for a single file."
                );

            // Guards the sum itself against overflow on a set of absurd declared lengths.
            declaredTotal += Math.Max(entry.Length, 0);

            if (declaredTotal > limits.MaxTotalUncompressedBytes)
                throw new InvalidDataException(
                    $"The archive unpacks to at least {declaredTotal:N0} bytes, above the "
                        + $"{limits.MaxTotalUncompressedBytes:N0} this instance accepts."
                );
        }
    }

    /// <summary>
    /// Wraps an entry's content so it cannot exceed either ceiling, whatever the archive claimed.
    /// The budget is the smaller of what one file may be and what is left of the archive's total.
    /// </summary>
    public Stream Bound(ZipArchiveEntry entry, Stream content)
    {
        var remainingTotal = limits.MaxTotalUncompressedBytes - _written;
        var budget = Math.Min(limits.MaxEntryBytes, Math.Max(remainingTotal, 0));

        return new BoundedStream(content, budget, entry.FullName);
    }

    /// <summary>Records what an entry actually cost, once it is safely written.</summary>
    public void Written(long bytes) => _written += bytes;

    /// <summary>
    /// Read-only pass-through that fails once more than <paramref name="budget"/> bytes have come
    /// out of it. Aborting during the read is the point: by the time a whole entry has been written
    /// the damage is already on disk.
    /// </summary>
    private sealed class BoundedStream(Stream inner, long budget, string entryName) : Stream
    {
        private long _read;

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => _read;
            set => throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count) =>
            Count(inner.Read(buffer, offset, count));

        public override int Read(Span<byte> buffer) => Count(inner.Read(buffer));

        public override async ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default
        ) => Count(await inner.ReadAsync(buffer, cancellationToken));

        public override async Task<int> ReadAsync(
            byte[] buffer,
            int offset,
            int count,
            CancellationToken cancellationToken
        ) => Count(await inner.ReadAsync(buffer.AsMemory(offset, count), cancellationToken));

        private int Count(int read)
        {
            _read += read;

            if (_read > budget)
                throw new InvalidDataException(
                    $"'{entryName}' is larger than the archive's remaining expansion budget of "
                        + $"{budget:N0} bytes. The archive understated its own size."
                );

            return read;
        }

        public override void Flush() { }

        public override long Seek(long offset, SeekOrigin origin) =>
            throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) =>
            throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            // The entry stream belongs to the ZipArchive and is disposed by the caller's `await
            // using`; disposing it here as well would close it twice.
            base.Dispose(disposing);
        }
    }
}
