using AwesomeAssertions;
using Fip.Strive.Application.Features.Jobs.Models;
using Fip.Strive.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.IntegrationTests;

[Collection(PostgresCollection.Name)]
public class UnpackJobTests(PostgresFixture postgres)
{
    [Fact]
    public async Task An_unpack_job_imports_the_archive_and_clears_the_staging_area()
    {
        await using var harness = await UnpackHarness.CreateAsync(postgres);

        var archive = ZipBuilder.Create(
            harness.ArchiveDirectory,
            "export.zip",
            ("activity/day-1.json", "one"),
            ("activity/day-2.json", "two")
        );

        await harness.StageAndEnqueueAsync(archive);
        await harness.Jobs.RunUntilIdleAsync();

        await using var context = harness.Jobs.CreateContext();

        (await context.Jobs.SingleAsync()).State.Should().Be(JobState.Succeeded);
        (await context.ImportPackages.CountAsync()).Should().Be(1);
        (await context.CatalogEntries.CountAsync()).Should().Be(2);
        (await context.PackageFiles.CountAsync()).Should().Be(2);

        Directory
            .EnumerateFiles(harness.Paths.Incoming)
            .Should()
            .BeEmpty("the archive is redundant once its contents are in the blob store");
    }

    [Fact]
    public async Task The_job_is_keyed_by_the_archive_hash()
    {
        await using var harness = await UnpackHarness.CreateAsync(postgres);

        var archive = ZipBuilder.Create(harness.ArchiveDirectory, "export.zip", ("a.json", "one"));

        var hash = await harness.StageAndEnqueueAsync(archive);
        await harness.Jobs.RunUntilIdleAsync();

        await using var context = harness.Jobs.CreateContext();
        var job = await context.Jobs.SingleAsync();

        job.Kind.Should().Be("unpack");
        job.TargetKey.Should().Be(hash);
        job.ComponentId.Should().Be("unpack");
        job.ComponentVersion.Should().Be(1);
    }

    [Fact]
    public async Task Re_uploading_an_archive_reuses_its_work_unit_and_does_no_work()
    {
        await using var harness = await UnpackHarness.CreateAsync(postgres);

        var archive = ZipBuilder.Create(harness.ArchiveDirectory, "export.zip", ("a.json", "one"));

        await harness.StageAndEnqueueAsync(archive);
        await harness.Jobs.RunUntilIdleAsync();

        await harness.StageAndEnqueueAsync(archive);
        await harness.Jobs.RunUntilIdleAsync();

        await using var context = harness.Jobs.CreateContext();

        (await context.Jobs.CountAsync()).Should().Be(1, "one archive is one work unit");
        (await context.ImportPackages.CountAsync()).Should().Be(1);
        (await context.PackageFiles.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task A_failed_unpack_keeps_the_staged_archive_so_a_retry_can_read_it()
    {
        await using var harness = await UnpackHarness.CreateAsync(postgres);

        var archive = ZipBuilder.Create(harness.ArchiveDirectory, "export.zip", ("a.json", "one"));
        await harness.StageAndEnqueueAsync(archive);

        // Corrupt the staged copy so unpacking throws where a real IO fault would.
        var staged = Directory.EnumerateFiles(harness.Paths.Incoming).Single();
        await File.WriteAllTextAsync(staged, "this is not a zip");

        await harness.Jobs.RunUntilIdleAsync();

        await using var context = harness.Jobs.CreateContext();
        var job = await context.Jobs.SingleAsync();

        job.State.Should().Be(JobState.Failed);
        job.Error.Should().NotBeNullOrWhiteSpace();

        File.Exists(staged).Should().BeTrue("a retry has to have something to read");
    }

    [Fact]
    public async Task Unpacking_reports_progress_against_the_job()
    {
        await using var harness = await UnpackHarness.CreateAsync(postgres);

        var files = Enumerable
            .Range(0, 25)
            .Select(index => ($"activity/day-{index}.json", $"payload {index}"))
            .ToArray();

        var archive = ZipBuilder.Create(harness.ArchiveDirectory, "export.zip", files);

        await harness.StageAndEnqueueAsync(archive);
        await harness.Jobs.RunUntilIdleAsync();

        await using var context = harness.Jobs.CreateContext();
        var job = await context.Jobs.SingleAsync();

        job.ProgressCurrent.Should().Be(25);
        job.ProgressTotal.Should().Be(25);
    }
}
