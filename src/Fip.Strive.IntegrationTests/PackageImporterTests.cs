using AwesomeAssertions;
using Fip.Strive.Application.Features.Import.Models;
using Fip.Strive.Application.Features.Storage;
using Fip.Strive.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.IntegrationTests;

/// <summary>
/// The importer's edges, as opposed to <c>ImportTests</c>' happy path: re-uploading the same bytes,
/// cancelling mid-import, and refusing an archive that expands past its ceiling.
/// </summary>
[Collection(PostgresCollection.Name)]
public class PackageImporterTests(PostgresFixture postgres)
{
    [Fact]
    public async Task Re_uploading_the_same_bytes_does_no_work()
    {
        await using var harness = await ImportHarness.CreateAsync(postgres);

        var archive = ZipBuilder.Create(
            harness.ArchiveDirectory,
            "export.zip",
            ("activity/day-1.json", "payload")
        );

        var first = await harness.ImportAsync(archive);
        var second = await harness.ImportAsync(archive);

        first.Outcome.Should().Be(ImportOutcome.Imported);

        second.Outcome.Should().Be(ImportOutcome.DuplicateArchive);
        second.PackageId.Should().Be(first.PackageId, "the original package is the answer");
        second.NewEntryCount.Should().Be(0);

        await using var context = harness.CreateContext();
        (await context.ImportPackages.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task The_same_content_under_a_different_archive_name_is_still_a_duplicate()
    {
        await using var harness = await ImportHarness.CreateAsync(postgres);

        // Identical bytes, different file name. Deduplication is on the archive hash, not the name.
        var first = ZipBuilder.Create(
            harness.ArchiveDirectory,
            "january.zip",
            ("activity/day-1.json", "payload")
        );

        var copy = Path.Combine(harness.ArchiveDirectory, "renamed.zip");
        File.Copy(first, copy);

        await harness.ImportAsync(first);
        var second = await harness.ImportAsync(copy);

        second.Outcome.Should().Be(ImportOutcome.DuplicateArchive);
    }

    [Fact]
    public async Task Cancelling_leaves_no_package_behind()
    {
        await using var harness = await ImportHarness.CreateAsync(postgres);

        var archive = ZipBuilder.Create(
            harness.ArchiveDirectory,
            "export.zip",
            ("activity/day-1.json", "one"),
            ("activity/day-2.json", "two"),
            ("activity/day-3.json", "three")
        );

        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        var act = async () => await harness.ImportAsync(archive, cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();

        await using var context = harness.CreateContext();

        // The catalog transaction never opened, so there is nothing half-written. Blobs may exist
        // and that is by design — C5 — they are unreferenced bytes a retry deduplicates against.
        (await context.ImportPackages.CountAsync())
            .Should()
            .Be(0);
        (await context.PackageFiles.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task A_cancelled_import_can_simply_be_retried()
    {
        await using var harness = await ImportHarness.CreateAsync(postgres);

        var archive = ZipBuilder.Create(
            harness.ArchiveDirectory,
            "export.zip",
            ("activity/day-1.json", "one"),
            ("activity/day-2.json", "two")
        );

        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        try
        {
            await harness.ImportAsync(archive, cancellation.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected — the point is what happens next.
        }

        var retry = await harness.ImportAsync(archive);

        retry.Outcome.Should().Be(ImportOutcome.Imported);
        retry.FileCount.Should().Be(2);
    }

    [Fact]
    public async Task An_archive_larger_than_one_insert_batch_is_recorded_whole()
    {
        await using var harness = await ImportHarness.CreateAsync(postgres);

        // Over the 5,000-row insert batch, so the manifest is written across several round-trips
        // inside one transaction. Below that boundary the batching path never runs, and every other
        // test in this suite sits below it.
        const int fileCount = 5_100;

        var entries = Enumerable
            .Range(0, fileCount)
            .Select(index => ($"activity/day-{index}.json", $"payload {index}"))
            .ToArray();

        var archive = ZipBuilder.Create(harness.ArchiveDirectory, "big.zip", entries);

        var result = await harness.ImportAsync(archive);

        result.Outcome.Should().Be(ImportOutcome.Imported);
        result.FileCount.Should().Be(fileCount);
        result.NewEntryCount.Should().Be(fileCount, "every payload is distinct");

        await using var context = harness.CreateContext();

        // The point of the transaction: all of it, or none of it. A partial manifest would be the
        // half-catalogued package the single-transaction design exists to prevent.
        (await context.PackageFiles.CountAsync())
            .Should()
            .Be(fileCount);
        (await context.CatalogEntries.CountAsync()).Should().Be(fileCount);
        (await context.ImportPackages.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task An_archive_past_its_expansion_ceiling_is_refused()
    {
        await using var harness = await ImportHarness.CreateAsync(postgres);

        // Lowered rather than crafting a genuinely enormous archive.
        harness.Limits = new StorageOptions { MaxTotalUncompressedBytes = 8 };

        var archive = ZipBuilder.Create(
            harness.ArchiveDirectory,
            "bomb.zip",
            ("activity/day-1.json", "considerably more than eight bytes")
        );

        var act = async () => await harness.ImportAsync(archive);

        await act.Should().ThrowAsync<InvalidDataException>();

        await using var context = harness.CreateContext();
        (await context.ImportPackages.CountAsync()).Should().Be(0);
    }
}
