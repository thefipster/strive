using System.IO.Compression;
using AwesomeAssertions;
using Fip.Strive.Application.Features.Import.Services;
using Fip.Strive.Application.Features.Storage;

namespace Fip.Strive.Application.UnitTests.Import;

/// <summary>
/// The ceiling on what an archive may expand into. Built against real <see cref="ZipArchive"/>
/// instances rather than stubs, because the thing being defended against is a real archive whose
/// declared sizes disagree with its contents — and only a real one can disagree.
/// </summary>
public class ExpansionGuardTests
{
    /// <summary>A megabyte of zeros, which is what makes a bomb a bomb: it compresses to nearly nothing.</summary>
    private static readonly byte[] Zeros = new byte[1024 * 1024];

    [Fact]
    public void An_ordinary_archive_passes()
    {
        using var zip = Open(BuildArchive(("notes.txt", 16), ("data.bin", 4096)));
        var guard = new ExpansionGuard(new StorageOptions());

        var act = () => guard.CheckDeclared(zip.Entries);

        act.Should().NotThrow();
    }

    [Fact]
    public void Too_many_entries_is_refused_before_anything_is_written()
    {
        using var zip = Open(BuildArchive(("a.txt", 1), ("b.txt", 1), ("c.txt", 1)));
        var guard = new ExpansionGuard(new StorageOptions { MaxArchiveEntries = 2 });

        var act = () => guard.CheckDeclared(zip.Entries);

        act.Should().Throw<InvalidDataException>().WithMessage("*3*above*2*");
    }

    [Fact]
    public void A_single_oversized_entry_is_refused()
    {
        using var zip = Open(BuildArchive(("big.bin", 4096)));
        var guard = new ExpansionGuard(new StorageOptions { MaxEntryBytes = 1024 });

        var act = () => guard.CheckDeclared(zip.Entries);

        act.Should().Throw<InvalidDataException>().WithMessage("*big.bin*");
    }

    [Fact]
    public void Entries_that_are_individually_fine_are_refused_in_total()
    {
        using var zip = Open(BuildArchive(("a.bin", 4096), ("b.bin", 4096), ("c.bin", 4096)));

        var guard = new ExpansionGuard(
            new StorageOptions { MaxEntryBytes = 8192, MaxTotalUncompressedBytes = 10_000 }
        );

        var act = () => guard.CheckDeclared(zip.Entries);

        act.Should().Throw<InvalidDataException>().WithMessage("*unpacks to at least*");
    }

    [Fact]
    public async Task An_entry_that_understates_its_size_is_aborted_mid_read()
    {
        // The declared-size pre-flight cannot catch this: the archive claims one megabyte and
        // delivers one megabyte, but the budget it is being measured against is far smaller. This
        // is the case that only the counting stream stops.
        using var zip = Open(BuildArchive(("bomb.bin", Zeros.Length)));
        var guard = new ExpansionGuard(new StorageOptions { MaxEntryBytes = 4096 });
        var entry = zip.Entries.Single();

        await using var content = entry.Open();
        await using var bounded = guard.Bound(entry, content);

        var act = async () => await bounded.CopyToAsync(Stream.Null);

        await act.Should().ThrowAsync<InvalidDataException>().WithMessage("*bomb.bin*");
    }

    [Fact]
    public async Task The_budget_shrinks_as_the_archive_is_consumed()
    {
        using var zip = Open(BuildArchive(("first.bin", 4096), ("second.bin", 4096)));

        var guard = new ExpansionGuard(
            new StorageOptions { MaxEntryBytes = 8192, MaxTotalUncompressedBytes = 6000 }
        );

        // The first entry fits and spends most of the archive's total.
        var first = zip.Entries[0];
        await using (var content = first.Open())
        await using (var bounded = guard.Bound(first, content))
            await bounded.CopyToAsync(Stream.Null);

        guard.Written(4096);

        // The second is the same size and well under MaxEntryBytes, but the archive has only
        // 1,904 bytes of total budget left.
        var second = zip.Entries[1];
        await using var secondContent = second.Open();
        await using var secondBounded = guard.Bound(second, secondContent);

        var act = async () => await secondBounded.CopyToAsync(Stream.Null);

        await act.Should().ThrowAsync<InvalidDataException>().WithMessage("*second.bin*");
    }

    [Fact]
    public async Task A_bounded_entry_within_budget_reads_through_unchanged()
    {
        using var zip = Open(BuildArchive(("fine.bin", 4096)));
        var guard = new ExpansionGuard(new StorageOptions());
        var entry = zip.Entries.Single();

        await using var content = entry.Open();
        await using var bounded = guard.Bound(entry, content);
        using var copy = new MemoryStream();

        await bounded.CopyToAsync(copy);

        copy.Length.Should().Be(4096, "bounding must not truncate what it allows");
    }

    private static ZipArchive Open(byte[] archive) =>
        new(new MemoryStream(archive), ZipArchiveMode.Read);

    private static byte[] BuildArchive(params (string Name, int Size)[] files)
    {
        var buffer = new MemoryStream();

        using (var zip = new ZipArchive(buffer, ZipArchiveMode.Create, leaveOpen: true))
            foreach (var (name, size) in files)
            {
                using var stream = zip.CreateEntry(name).Open();
                stream.Write(new byte[size]);
            }

        return buffer.ToArray();
    }
}
