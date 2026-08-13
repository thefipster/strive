using AwesomeAssertions;
using Fip.Strive.IntegrationTests.Fixtures;

namespace Fip.Strive.IntegrationTests;

/// <summary>
/// The catalog browser's read side, which had no coverage at all. Against real Postgres rather than
/// an in-memory provider, because the whole point of these assertions is what the *database* does
/// with a `LIKE` and a prefix match — an in-memory provider would answer with LINQ-to-objects
/// semantics and prove nothing.
/// </summary>
[Collection(PostgresCollection.Name)]
public class CatalogReaderTests(PostgresFixture postgres)
{
    [Fact]
    public async Task An_uppercase_hash_finds_its_entry()
    {
        await using var harness = await ImportHarness.CreateAsync(postgres);

        var archive = ZipBuilder.Create(
            harness.ArchiveDirectory,
            "export.zip",
            ("activity/day-1.json", "shared payload")
        );

        await harness.ImportAsync(archive);

        await using var context = harness.CreateContext();
        var reader = harness.CreateCatalogReader(context);

        var stored = (await reader.GetEntriesAsync(0, 10)).Items.Single().Hash;

        // Hashes are stored lowercase. Pasting one from a tool that emits uppercase used to match
        // nothing, with nothing to say why.
        var found = await reader.GetEntriesAsync(0, 10, stored.ToUpperInvariant());

        found.Items.Should().ContainSingle().Which.Hash.Should().Be(stored);
    }

    [Fact]
    public async Task A_hash_prefix_is_enough()
    {
        await using var harness = await ImportHarness.CreateAsync(postgres);

        var archive = ZipBuilder.Create(
            harness.ArchiveDirectory,
            "export.zip",
            ("activity/day-1.json", "shared payload")
        );

        await harness.ImportAsync(archive);

        await using var context = harness.CreateContext();
        var reader = harness.CreateCatalogReader(context);

        var stored = (await reader.GetEntriesAsync(0, 10)).Items.Single().Hash;
        var found = await reader.GetEntriesAsync(0, 10, stored[..8]);

        found.Items.Should().ContainSingle().Which.Hash.Should().Be(stored);
    }

    [Fact]
    public async Task A_path_fragment_finds_every_entry_filed_under_it()
    {
        await using var harness = await ImportHarness.CreateAsync(postgres);

        var archive = ZipBuilder.Create(
            harness.ArchiveDirectory,
            "export.zip",
            ("activity/day-1.json", "one"),
            ("activity/day-2.json", "two"),
            ("backup/notes.txt", "three")
        );

        await harness.ImportAsync(archive);

        await using var context = harness.CreateContext();
        var reader = harness.CreateCatalogReader(context);

        var found = await reader.GetEntriesAsync(0, 10, "activity/");

        found.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task A_search_matching_nothing_returns_an_empty_page_rather_than_everything()
    {
        await using var harness = await ImportHarness.CreateAsync(postgres);

        var archive = ZipBuilder.Create(
            harness.ArchiveDirectory,
            "export.zip",
            ("activity/day-1.json", "one")
        );

        await harness.ImportAsync(archive);

        await using var context = harness.CreateContext();
        var reader = harness.CreateCatalogReader(context);

        var found = await reader.GetEntriesAsync(0, 10, "nothing-matches-this");

        found.Items.Should().BeEmpty();
        found.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task The_summary_counts_packages_entries_and_bytes()
    {
        await using var harness = await ImportHarness.CreateAsync(postgres);

        var archive = ZipBuilder.Create(
            harness.ArchiveDirectory,
            "export.zip",
            ("activity/day-1.json", "one"),
            ("activity/day-2.json", "two")
        );

        await harness.ImportAsync(archive);

        await using var context = harness.CreateContext();
        var reader = harness.CreateCatalogReader(context);

        var summary = await reader.GetSummaryAsync();

        summary.PackageCount.Should().Be(1);
        summary.EntryCount.Should().Be(2);
        summary.TotalBytes.Should().Be(6, "'one' and 'two' are three bytes each");
    }

    [Fact]
    public async Task Occurrences_list_every_path_a_blob_was_filed_under()
    {
        await using var harness = await ImportHarness.CreateAsync(postgres);

        var archive = ZipBuilder.Create(
            harness.ArchiveDirectory,
            "export.zip",
            ("activity/day-1.json", "shared payload"),
            ("backup/day-1-copy.json", "shared payload")
        );

        await harness.ImportAsync(archive);

        await using var context = harness.CreateContext();
        var reader = harness.CreateCatalogReader(context);

        var hash = (await reader.GetEntriesAsync(0, 10)).Items.Single().Hash;
        var occurrences = await reader.GetOccurrencesAsync(hash);

        occurrences
            .Select(occurrence => occurrence.PathInArchive)
            .Should()
            .BeEquivalentTo(["activity/day-1.json", "backup/day-1-copy.json"]);
    }
}
