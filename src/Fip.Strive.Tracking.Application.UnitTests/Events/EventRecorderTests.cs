using AwesomeAssertions;
using Fip.Strive.Tracking.Application.Features.Events.Models;
using Fip.Strive.Tracking.Application.Features.Events.Services;
using Fip.Strive.Tracking.Application.Features.Trackers.Models;
using Fip.Strive.Tracking.Application.Features.Trackers.Services;
using Fip.Strive.Tracking.Application.Infrastructure;
using Fip.Strive.Tracking.Application.UnitTests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.Tracking.Application.UnitTests.Events;

public class EventRecorderTests : IDisposable
{
    private readonly TrackingDatabaseFixture _database = new();

    [Fact]
    public async Task Both_kinds_of_value_survive_the_round_trip()
    {
        var (trackerId, cups, mood) = await SetUpAsync();

        await RecordAsync(
            trackerId,
            new NewEvent(
                DateTimeOffset.UtcNow,
                "morning",
                [new NewEventValue(cups, 2.5m), new NewEventValue(mood, Text: "  fine  ")]
            )
        );

        await using var context = _database.NewContext();
        var page = await new EventReader(context).GetEventsAsync(trackerId, 0, 10);

        page.TotalCount.Should().Be(1);
        page.Items[0].Note.Should().Be("morning");
        page.Items[0].ValueFor(cups)!.Number.Should().Be(2.5m);
        page.Items[0].ValueFor(mood)!.Text.Should().Be("fine");
    }

    [Fact]
    public async Task An_optional_field_left_blank_records_no_value_at_all()
    {
        var (trackerId, cups, _) = await SetUpAsync();

        await RecordAsync(trackerId, new NewEvent(DateTimeOffset.UtcNow));

        await using var context = _database.NewContext();
        (await context.TrackerEventValues.CountAsync()).Should().Be(0);

        var page = await new EventReader(context).GetEventsAsync(trackerId, 0, 10);
        page.Items[0].ValueFor(cups).Should().BeNull();
    }

    [Fact]
    public async Task A_required_field_left_blank_is_rejected()
    {
        var (trackerId, _, _) = await SetUpAsync(requiredCups: true);

        var record = async () => await RecordAsync(trackerId, new NewEvent(DateTimeOffset.UtcNow));

        await record.Should().ThrowAsync<TrackingException>();

        await using var context = _database.NewContext();
        (await context.TrackerEvents.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task A_value_for_a_field_of_another_tracker_is_ignored()
    {
        var (trackerId, cups, _) = await SetUpAsync();
        var (otherId, otherField, _) = await SetUpAsync("Gym");

        await RecordAsync(
            trackerId,
            new NewEvent(
                DateTimeOffset.UtcNow,
                Values: [new NewEventValue(cups, 1m), new NewEventValue(otherField, 99m)]
            )
        );

        await using var context = _database.NewContext();
        var page = await new EventReader(context).GetEventsAsync(trackerId, 0, 10);

        page.Items[0].Values.Should().ContainSingle();
        page.Items[0].ValueFor(cups)!.Number.Should().Be(1m);

        var other = await new EventReader(context).GetEventsAsync(otherId, 0, 10);
        other.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task A_time_in_another_offset_is_stored_as_the_same_instant_in_utc()
    {
        var (trackerId, _, _) = await SetUpAsync();
        var berlinNoon = new DateTimeOffset(2026, 8, 13, 12, 0, 0, TimeSpan.FromHours(2));

        await RecordAsync(trackerId, new NewEvent(berlinNoon));

        await using var context = _database.NewContext();
        var stored = await context.TrackerEvents.AsNoTracking().SingleAsync();

        stored.OccurredUtc.Offset.Should().Be(TimeSpan.Zero);
        stored.OccurredUtc.Should().Be(berlinNoon);
        stored.OccurredUtc.Hour.Should().Be(10);
    }

    [Fact]
    public async Task Deleting_an_event_drops_its_values_too()
    {
        var (trackerId, cups, _) = await SetUpAsync();

        var eventId = await RecordAsync(
            trackerId,
            new NewEvent(DateTimeOffset.UtcNow, Values: [new NewEventValue(cups, 3m)])
        );

        await using (var context = _database.NewContext())
            await new EventRecorder(context, TimeProvider.System).DeleteAsync(eventId);

        await using var verifying = _database.NewContext();
        (await verifying.TrackerEvents.CountAsync()).Should().Be(0);
        (await verifying.TrackerEventValues.CountAsync()).Should().Be(0);
    }

    /// <summary>A tracker with one number field and one text field.</summary>
    private async Task<(Guid TrackerId, Guid Cups, Guid Mood)> SetUpAsync(
        string name = "Coffee",
        bool requiredCups = false
    )
    {
        await using var context = _database.NewContext();
        var writer = new TrackerWriter(context, TimeProvider.System);

        var trackerId = await writer.CreateTrackerAsync(new NewTracker(name));
        var cups = await writer.AddFieldAsync(
            trackerId,
            new NewField("Cups", TrackerFieldType.Number, "cups", requiredCups)
        );
        var mood = await writer.AddFieldAsync(trackerId, new NewField("Mood", TrackerFieldType.Text));

        return (trackerId, cups, mood);
    }

    private async Task<Guid> RecordAsync(Guid trackerId, NewEvent entry)
    {
        await using var context = _database.NewContext();
        return await new EventRecorder(context, TimeProvider.System).RecordAsync(trackerId, entry);
    }

    public void Dispose() => _database.Dispose();
}
