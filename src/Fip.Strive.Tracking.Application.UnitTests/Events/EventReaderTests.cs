using AwesomeAssertions;
using Fip.Strive.Tracking.Application.Features.Events.Models;
using Fip.Strive.Tracking.Application.Features.Events.Services;
using Fip.Strive.Tracking.Application.Features.Trackers.Models;
using Fip.Strive.Tracking.Application.Features.Trackers.Services;
using Fip.Strive.Tracking.Application.UnitTests.Fixtures;

namespace Fip.Strive.Tracking.Application.UnitTests.Events;

public class EventReaderTests : IDisposable
{
    private static readonly DateTimeOffset Noon = new(2026, 8, 13, 12, 0, 0, TimeSpan.Zero);

    private readonly TrackingDatabaseFixture _database = new();

    [Fact]
    public async Task Events_come_back_newest_first_and_page()
    {
        var (trackerId, cups) = await SetUpAsync();

        for (var hour = 0; hour < 5; hour++)
            await RecordAsync(trackerId, Noon.AddHours(hour), cups, hour);

        await using var context = _database.NewContext();
        var reader = new EventReader(context);

        var first = await reader.GetEventsAsync(trackerId, 0, 2);
        first.TotalCount.Should().Be(5);
        first
            .Items.Select(item => item.OccurredUtc)
            .Should()
            .Equal(Noon.AddHours(4), Noon.AddHours(3));

        var second = await reader.GetEventsAsync(trackerId, 2, 2);
        second
            .Items.Select(item => item.OccurredUtc)
            .Should()
            .Equal(Noon.AddHours(2), Noon.AddHours(1));
    }

    [Fact]
    public async Task Number_stats_are_totalled_over_every_event()
    {
        var (trackerId, cups) = await SetUpAsync();

        await RecordAsync(trackerId, Noon, cups, 2);
        await RecordAsync(trackerId, Noon.AddHours(1), cups, 3);
        await RecordAsync(trackerId, Noon.AddHours(2), cups, null);

        await using var context = _database.NewContext();
        var stats = await new EventReader(context).GetNumberStatsAsync(trackerId);

        stats.Should().ContainSingle();
        stats[0].FieldName.Should().Be("Cups");
        stats[0].Unit.Should().Be("cups");
        stats[0].Count.Should().Be(2);
        stats[0].Total.Should().Be(5m);
        stats[0].Average.Should().Be(2.5m);
        stats[0].Minimum.Should().Be(2m);
        stats[0].Maximum.Should().Be(3m);
    }

    [Fact]
    public async Task Fractional_values_survive_the_aggregate()
    {
        var (trackerId, cups) = await SetUpAsync();

        // Aggregation happens in SQLite over a REAL cast, so this is the case worth pinning: the
        // values are stored as text, and totalling them as text would return nonsense.
        await RecordAsync(trackerId, Noon, cups, 1.5m);
        await RecordAsync(trackerId, Noon.AddHours(1), cups, 2.25m);
        await RecordAsync(trackerId, Noon.AddHours(2), cups, 0.25m);

        await using var context = _database.NewContext();
        var stats = await new EventReader(context).GetNumberStatsAsync(trackerId);

        stats[0].Count.Should().Be(3);
        stats[0].Total.Should().Be(4m);
        stats[0].Minimum.Should().Be(0.25m);
        stats[0].Maximum.Should().Be(2.25m);
    }

    [Fact]
    public async Task Values_sort_numerically_rather_than_as_text()
    {
        var (trackerId, cups) = await SetUpAsync();

        // The trap the REAL cast avoids: as text, "10" sorts before "9", so MIN and MAX would both
        // be wrong in a way nobody would notice until the numbers crossed a digit boundary.
        await RecordAsync(trackerId, Noon, cups, 9m);
        await RecordAsync(trackerId, Noon.AddHours(1), cups, 10m);

        await using var context = _database.NewContext();
        var stats = await new EventReader(context).GetNumberStatsAsync(trackerId);

        stats[0].Minimum.Should().Be(9m);
        stats[0].Maximum.Should().Be(10m);
    }

    [Fact]
    public async Task A_number_field_nobody_filled_in_produces_no_statistics()
    {
        var (trackerId, cups) = await SetUpAsync();

        await RecordAsync(trackerId, Noon, cups, null);

        await using var context = _database.NewContext();
        var stats = await new EventReader(context).GetNumberStatsAsync(trackerId);

        stats.Should().BeEmpty();
    }

    [Fact]
    public async Task A_field_added_after_an_event_leaves_that_event_blank_rather_than_hidden()
    {
        var (trackerId, _) = await SetUpAsync();
        await RecordAsync(trackerId, Noon, null, null);

        Guid late;

        await using (var context = _database.NewContext())
            late = await new TrackerWriter(context, TimeProvider.System).AddFieldAsync(
                trackerId,
                new NewField("Sugar", TrackerFieldType.Number)
            );

        await using var reading = _database.NewContext();
        var page = await new EventReader(reading).GetEventsAsync(trackerId, 0, 10);

        page.Items.Should().ContainSingle();
        page.Items[0].ValueFor(late).Should().BeNull();
    }

    private async Task<(Guid TrackerId, Guid Cups)> SetUpAsync()
    {
        await using var context = _database.NewContext();
        var writer = new TrackerWriter(context, TimeProvider.System);

        var trackerId = await writer.CreateTrackerAsync(new NewTracker("Coffee"));
        var cups = await writer.AddFieldAsync(
            trackerId,
            new NewField("Cups", TrackerFieldType.Number, "cups")
        );

        return (trackerId, cups);
    }

    private async Task RecordAsync(
        Guid trackerId,
        DateTimeOffset occurred,
        Guid? field,
        decimal? cups
    )
    {
        await using var context = _database.NewContext();

        var values = new List<NewEventValue>();

        if (field is { } fieldId && cups is { } number)
            values.Add(new NewEventValue(fieldId, number));

        await new EventRecorder(context, TimeProvider.System).RecordAsync(
            trackerId,
            new NewEvent(occurred, Values: values)
        );
    }

    public void Dispose() => _database.Dispose();
}
