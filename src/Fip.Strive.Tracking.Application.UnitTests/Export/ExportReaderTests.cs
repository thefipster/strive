using AwesomeAssertions;
using Fip.Strive.Tracking.Application.Features.Events.Models;
using Fip.Strive.Tracking.Application.Features.Events.Services;
using Fip.Strive.Tracking.Application.Features.Export.Services;
using Fip.Strive.Tracking.Application.Features.Export.Services.Contracts;
using Fip.Strive.Tracking.Application.Features.Trackers.Models;
using Fip.Strive.Tracking.Application.Features.Trackers.Services;
using Fip.Strive.Tracking.Application.UnitTests.Fixtures;

namespace Fip.Strive.Tracking.Application.UnitTests.Export;

public class ExportReaderTests : IDisposable
{
    private static readonly DateTimeOffset Start = new(2026, 8, 13, 6, 0, 0, TimeSpan.Zero);

    private readonly TrackingDatabaseFixture _database = new();
    private readonly StubClock _clock = new(Start);

    [Fact]
    public async Task Trackers_come_back_with_their_fields_in_order()
    {
        var trackerId = await NewTrackerAsync("Coffee");
        await NewFieldAsync(trackerId, "Cups", TrackerFieldType.Number, "cups");
        await NewFieldAsync(trackerId, "Mood", TrackerFieldType.Text);

        await using var context = _database.NewContext();
        var trackers = await new ExportReader(context).GetTrackersAsync();

        trackers.Should().ContainSingle();
        trackers[0].Name.Should().Be("Coffee");
        trackers[0].Fields.Select(field => field.Name).Should().Equal("Cups", "Mood");
        trackers[0].Fields[0].Type.Should().Be(TrackerFieldType.Number);
        trackers[0].Fields[0].Unit.Should().Be("cups");
    }

    [Fact]
    public async Task Events_come_back_oldest_first_so_a_puller_can_walk_forwards()
    {
        var trackerId = await NewTrackerAsync("Coffee");

        for (var index = 0; index < 3; index++)
        {
            await RecordAsync(trackerId, Start.AddDays(-index));
            _clock.Advance(TimeSpan.FromMinutes(1));
        }

        await using var context = _database.NewContext();
        var page = await new ExportReader(context).GetEventsAsync();

        page.TotalCount.Should().Be(3);

        // Recorded in order even though they happened in reverse — the feed is ordered by when the
        // row was written, not by when the thing happened.
        page.Items.Select(item => item.RecordedUtc)
            .Should()
            .Equal(Start, Start.AddMinutes(1), Start.AddMinutes(2));

        page.Items.Select(item => item.OccurredUtc)
            .Should()
            .Equal(Start, Start.AddDays(-1), Start.AddDays(-2));
    }

    [Fact]
    public async Task Since_filters_on_when_the_row_was_written_and_includes_the_boundary()
    {
        var trackerId = await NewTrackerAsync("Coffee");

        await RecordAsync(trackerId, Start);
        _clock.Advance(TimeSpan.FromMinutes(1));
        await RecordAsync(trackerId, Start);
        _clock.Advance(TimeSpan.FromMinutes(1));
        await RecordAsync(trackerId, Start);

        await using var context = _database.NewContext();
        var page = await new ExportReader(context).GetEventsAsync(since: Start.AddMinutes(1));

        page.TotalCount.Should().Be(2);
        page.Items.Select(item => item.RecordedUtc)
            .Should()
            .Equal(Start.AddMinutes(1), Start.AddMinutes(2));
    }

    [Fact]
    public async Task A_backdated_event_still_reaches_a_puller_that_synced_past_its_date()
    {
        var trackerId = await NewTrackerAsync("Coffee");

        // Entered today, but for something that happened a week ago. Filtering on OccurredUtc would
        // lose it; this is the whole reason the cursor is RecordedUtc.
        _clock.Advance(TimeSpan.FromDays(1));
        await RecordAsync(trackerId, Start.AddDays(-7));

        await using var context = _database.NewContext();
        var page = await new ExportReader(context).GetEventsAsync(since: Start);

        page.Items.Should().ContainSingle();
        page.Items[0].OccurredUtc.Should().Be(Start.AddDays(-7));
    }

    [Fact]
    public async Task NextSince_points_at_the_newest_row_in_the_page()
    {
        var trackerId = await NewTrackerAsync("Coffee");

        await RecordAsync(trackerId, Start);
        _clock.Advance(TimeSpan.FromMinutes(1));
        await RecordAsync(trackerId, Start);

        await using var context = _database.NewContext();
        var reader = new ExportReader(context);

        var first = await reader.GetEventsAsync(take: 1);
        first.NextSince.Should().Be(Start);

        // Inclusive, so the cursor's own event arrives again — callers dedupe on id.
        var second = await reader.GetEventsAsync(since: first.NextSince);
        second.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task An_empty_page_hands_the_callers_own_cursor_back()
    {
        await NewTrackerAsync("Coffee");

        await using var context = _database.NewContext();
        var page = await new ExportReader(context).GetEventsAsync(since: Start.AddDays(1));

        page.Items.Should().BeEmpty();
        page.NextSince.Should().Be(Start.AddDays(1));
    }

    [Fact]
    public async Task One_tracker_can_be_pulled_on_its_own()
    {
        var coffee = await NewTrackerAsync("Coffee");
        var gym = await NewTrackerAsync("Gym");

        await RecordAsync(coffee, Start);
        _clock.Advance(TimeSpan.FromMinutes(1));
        await RecordAsync(gym, Start);

        await using var context = _database.NewContext();
        var page = await new ExportReader(context).GetEventsAsync(trackerId: gym);

        page.TotalCount.Should().Be(1);
        page.Items[0].TrackerName.Should().Be("Gym");
    }

    [Fact]
    public async Task Values_travel_with_the_event()
    {
        var trackerId = await NewTrackerAsync("Coffee");
        var cups = await NewFieldAsync(trackerId, "Cups", TrackerFieldType.Number, "cups");

        await RecordAsync(trackerId, Start, new NewEventValue(cups, 2.5m));

        await using var context = _database.NewContext();
        var page = await new ExportReader(context).GetEventsAsync();

        page.Items[0].Values.Should().ContainSingle();
        page.Items[0].Values[0].FieldName.Should().Be("Cups");
        page.Items[0].Values[0].Unit.Should().Be("cups");
        page.Items[0].Values[0].Number.Should().Be(2.5m);
        page.Items[0].Values[0].Type.Should().Be(TrackerFieldType.Number);
    }

    [Fact]
    public async Task A_nonsensical_page_size_is_clamped_rather_than_obeyed()
    {
        var trackerId = await NewTrackerAsync("Coffee");

        await RecordAsync(trackerId, Start);
        _clock.Advance(TimeSpan.FromMinutes(1));
        await RecordAsync(trackerId, Start);

        await using var context = _database.NewContext();
        var reader = new ExportReader(context);

        (await reader.GetEventsAsync(take: 0)).Items.Should().ContainSingle();
        (await reader.GetEventsAsync(take: int.MaxValue)).Items.Should().HaveCount(2);
        (await reader.GetEventsAsync(skip: -5)).Items.Should().HaveCount(2);
    }

    [Fact]
    public void The_page_size_ceiling_is_high_enough_to_be_worth_having()
    {
        IExportReader.DefaultPageSize.Should().BeLessThanOrEqualTo(IExportReader.MaximumPageSize);
    }

    private async Task<Guid> NewTrackerAsync(string name)
    {
        await using var context = _database.NewContext();
        return await new TrackerWriter(context, _clock).CreateTrackerAsync(new NewTracker(name));
    }

    private async Task<Guid> NewFieldAsync(
        Guid trackerId,
        string name,
        TrackerFieldType type,
        string? unit = null
    )
    {
        await using var context = _database.NewContext();
        return await new TrackerWriter(context, _clock).AddFieldAsync(
            trackerId,
            new NewField(name, type, unit)
        );
    }

    private async Task RecordAsync(
        Guid trackerId,
        DateTimeOffset occurred,
        params NewEventValue[] values
    )
    {
        await using var context = _database.NewContext();
        await new EventRecorder(context, _clock).RecordAsync(
            trackerId,
            new NewEvent(occurred, Values: values)
        );
    }

    public void Dispose() => _database.Dispose();
}
