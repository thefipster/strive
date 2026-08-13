using AwesomeAssertions;
using Fip.Strive.Application.Features.Import.Models;
using Fip.Strive.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.IntegrationTests;

/// <summary>
/// Step 1's done criterion, mechanised: two packages with overlapping content collapse to single
/// catalog entries, the manifests still record every occurrence, and re-uploading an archive does
/// no work.
/// </summary>
[Collection(PostgresCollection.Name)]
public class ImportTests(PostgresFixture postgres)
{
    [Fact]
    public async Task Migrations_produce_an_empty_catalog()
    {
        await using var harness = await ImportHarness.CreateAsync(postgres);
        await using var context = harness.CreateContext();

        (await context.Database.GetPendingMigrationsAsync()).Should().BeEmpty();
        (await context.CatalogEntries.CountAsync()).Should().Be(0);
        (await context.ImportPackages.CountAsync()).Should().Be(0);
        (await context.PackageFiles.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Overlapping_packages_collapse_to_single_catalog_entries()
    {
        await using var harness = await ImportHarness.CreateAsync(postgres);

        var first = ZipBuilder.Create(
            harness.ArchiveDirectory,
            "export-january.zip",
            ("activity/day-1.json", "shared payload"),
            ("activity/day-2.json", "january only")
        );

        var second = ZipBuilder.Create(
            harness.ArchiveDirectory,
            "export-february.zip",
            ("backup/day-1-copy.json", "shared payload"),
            ("activity/day-3.json", "february only")
        );

        var firstResult = await harness.ImportAsync(first);
        var secondResult = await harness.ImportAsync(second);

        firstResult.Outcome.Should().Be(ImportOutcome.Imported);
        firstResult.FileCount.Should().Be(2);
        firstResult.NewEntryCount.Should().Be(2);

        secondResult.Outcome.Should().Be(ImportOutcome.Imported);
        secondResult.FileCount.Should().Be(2);
        secondResult.NewEntryCount.Should().Be(1, "the shared payload was already catalogued");

        await using var context = harness.CreateContext();

        (await context.CatalogEntries.CountAsync())
            .Should()
            .Be(3, "four paths across two packages resolve to three distinct blobs");

        (await context.PackageFiles.CountAsync())
            .Should()
            .Be(4, "every path occurrence is still recorded");

        harness.CountBlobs().Should().Be(3, "the shared bytes are on disk exactly once");
    }

    [Fact]
    public async Task Manifests_record_every_path_the_content_appeared_under()
    {
        await using var harness = await ImportHarness.CreateAsync(postgres);

        var first = ZipBuilder.Create(
            harness.ArchiveDirectory,
            "one.zip",
            ("a/shared.json", "same bytes")
        );
        var second = ZipBuilder.Create(
            harness.ArchiveDirectory,
            "two.zip",
            ("b/shared-renamed.json", "same bytes")
        );

        await harness.ImportAsync(first);
        await harness.ImportAsync(second);

        await using var context = harness.CreateContext();
        var entry = await context.CatalogEntries.SingleAsync();

        var catalog = harness.CreateCatalogReader(context);
        var occurrences = await catalog.GetOccurrencesAsync(entry.Hash);

        occurrences.Should().HaveCount(2);
        occurrences
            .Select(occurrence => occurrence.PathInArchive)
            .Should()
            .BeEquivalentTo(["a/shared.json", "b/shared-renamed.json"]);
        occurrences
            .Select(occurrence => occurrence.PackageName)
            .Should()
            .BeEquivalentTo(["one.zip", "two.zip"]);
    }

    [Fact]
    public async Task Re_uploading_the_same_archive_is_detected_and_does_no_work()
    {
        await using var harness = await ImportHarness.CreateAsync(postgres);

        var archive = ZipBuilder.Create(
            harness.ArchiveDirectory,
            "export.zip",
            ("activity/day-1.json", "payload one"),
            ("activity/day-2.json", "payload two")
        );

        var first = await harness.ImportAsync(archive);
        var second = await harness.ImportAsync(archive);

        second.Outcome.Should().Be(ImportOutcome.DuplicateArchive);
        second.PackageId.Should().Be(first.PackageId, "the original package is reported back");
        second.NewEntryCount.Should().Be(0);

        await using var context = harness.CreateContext();

        (await context.ImportPackages.CountAsync()).Should().Be(1);
        (await context.PackageFiles.CountAsync()).Should().Be(2);
        (await context.CatalogEntries.CountAsync()).Should().Be(2);
    }

    [Fact]
    public async Task A_renamed_copy_of_a_known_archive_is_still_a_duplicate()
    {
        await using var harness = await ImportHarness.CreateAsync(postgres);

        var original = ZipBuilder.Create(
            harness.ArchiveDirectory,
            "export.zip",
            ("activity/day-1.json", "payload")
        );

        var renamed = Path.Combine(harness.ArchiveDirectory, "export-copy.zip");
        File.Copy(original, renamed);

        await harness.ImportAsync(original);
        var result = await harness.ImportAsync(renamed);

        result
            .Outcome.Should()
            .Be(ImportOutcome.DuplicateArchive, "identity is the bytes, not the name");
    }

    [Fact]
    public async Task Duplicate_content_inside_one_archive_is_stored_once_but_listed_twice()
    {
        await using var harness = await ImportHarness.CreateAsync(postgres);

        var archive = ZipBuilder.Create(
            harness.ArchiveDirectory,
            "export.zip",
            ("original/data.json", "identical"),
            ("backup/data.json", "identical")
        );

        var result = await harness.ImportAsync(archive);

        result.FileCount.Should().Be(2);
        result.NewEntryCount.Should().Be(1);

        await using var context = harness.CreateContext();

        (await context.CatalogEntries.CountAsync()).Should().Be(1);
        (await context.PackageFiles.CountAsync()).Should().Be(2);
        harness.CountBlobs().Should().Be(1);
    }

    [Fact]
    public async Task Directory_entries_are_not_catalogued()
    {
        await using var harness = await ImportHarness.CreateAsync(postgres);

        var archive = ZipBuilder.CreateWithDirectoryEntry(
            harness.ArchiveDirectory,
            "export.zip",
            "activity/",
            ("activity/day-1.json", "payload")
        );

        var result = await harness.ImportAsync(archive);

        result.FileCount.Should().Be(1, "a directory entry carries no content");

        await using var context = harness.CreateContext();
        (await context.CatalogEntries.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Package_rows_report_the_new_versus_known_split()
    {
        await using var harness = await ImportHarness.CreateAsync(postgres);

        var first = ZipBuilder.Create(
            harness.ArchiveDirectory,
            "first.zip",
            ("a.json", "one"),
            ("b.json", "two")
        );
        var second = ZipBuilder.Create(
            harness.ArchiveDirectory,
            "second.zip",
            ("a-copy.json", "one"),
            ("b-copy.json", "two"),
            ("c.json", "three")
        );

        await harness.ImportAsync(first);
        await harness.ImportAsync(second);

        await using var context = harness.CreateContext();
        var packages = await harness.CreateCatalogReader(context).GetPackagesAsync();

        packages.Should().HaveCount(2);

        var latest = packages.Single(package => package.FileName == "second.zip");
        latest.FileCount.Should().Be(3);
        latest.NewEntryCount.Should().Be(1);
        latest.KnownEntryCount.Should().Be(2);
    }

    [Fact]
    public async Task The_staging_area_is_left_empty_after_importing()
    {
        await using var harness = await ImportHarness.CreateAsync(postgres);

        var archive = ZipBuilder.Create(harness.ArchiveDirectory, "export.zip", ("a.json", "one"));

        await harness.ImportAsync(archive);

        Directory
            .EnumerateFiles(harness.Paths.Incoming)
            .Should()
            .BeEmpty("the archive is redundant once its contents are in the blob store");
    }

    [Fact]
    public async Task Catalog_summary_counts_distinct_content_not_occurrences()
    {
        await using var harness = await ImportHarness.CreateAsync(postgres);

        var archive = ZipBuilder.Create(
            harness.ArchiveDirectory,
            "export.zip",
            ("a.json", "12345"),
            ("b.json", "12345"),
            ("c.json", "678")
        );

        await harness.ImportAsync(archive);

        await using var context = harness.CreateContext();
        var summary = await harness.CreateCatalogReader(context).GetSummaryAsync();

        summary.PackageCount.Should().Be(1);
        summary.EntryCount.Should().Be(2);
        summary.TotalBytes.Should().Be(8, "5 bytes plus 3, counted once each");
    }
}
