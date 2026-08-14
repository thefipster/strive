using AwesomeAssertions;
using Fip.Strive.Application.Features.Jobs.Models;
using Fip.Strive.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.IntegrationTests;

/// <summary>
/// Step 2's done criterion, mechanised: a large unpack killed mid-run resumes on restart and
/// completes with no duplicate or lost work.
/// </summary>
[Collection(PostgresCollection.Name)]
public class JobRecoveryTests(PostgresFixture postgres)
{
    [Fact]
    public async Task A_run_killed_mid_unpack_resumes_and_completes_exactly_once()
    {
        await using var harness = await UnpackHarness.CreateAsync(postgres);

        // Large enough that the kill lands mid-unpack rather than after it.
        var files = Enumerable
            .Range(0, 400)
            .Select(index => ($"activity/day-{index}.json", $"payload number {index}"))
            .ToArray();

        var archive = ZipBuilder.Create(harness.ArchiveDirectory, "export.zip", files);
        await harness.StageAndEnqueueAsync(archive);

        await harness.Jobs.KillMidRunAsync();

        await using (var context = harness.Jobs.CreateContext())
        {
            (await context.Jobs.SingleAsync())
                .State.Should()
                .Be(JobState.Running, "a killed process leaves its claim behind");
        }

        // A fresh runner: startup recovery has to find the claim and finish the work.
        await harness.Jobs.RunUntilIdleAsync();

        await using var reader = harness.Jobs.CreateContext();

        (await reader.Jobs.SingleAsync()).State.Should().Be(JobState.Succeeded);

        (await reader.ImportPackages.CountAsync())
            .Should()
            .Be(1, "a resumed run must not import the archive twice");

        (await reader.PackageFiles.CountAsync()).Should().Be(400, "no file may be lost");

        (await reader.CatalogEntries.CountAsync())
            .Should()
            .Be(400, "every payload is distinct, and none may be duplicated");
    }

    [Fact]
    public async Task Nothing_is_left_claimed_after_a_restart()
    {
        await using var harness = await UnpackHarness.CreateAsync(postgres);

        var files = Enumerable
            .Range(0, 200)
            .Select(index => ($"a/{index}.json", $"payload {index}"))
            .ToArray();

        var archive = ZipBuilder.Create(harness.ArchiveDirectory, "export.zip", files);
        await harness.StageAndEnqueueAsync(archive);

        await harness.Jobs.KillMidRunAsync();
        await harness.Jobs.RunUntilIdleAsync();

        await using var reader = harness.Jobs.CreateContext();

        (await reader.Jobs.CountAsync(job => job.State == JobState.Running))
            .Should()
            .Be(0, "startup recovery re-queues anything a dead process left claimed");
    }
}
