using AwesomeAssertions;
using Fip.Strive.Tracking.Application.Features.Events.Models;
using Fip.Strive.Tracking.Application.Features.Events.Services;
using Fip.Strive.Tracking.Application.Features.Trackers.Models;
using Fip.Strive.Tracking.Application.Features.Trackers.Services;
using Fip.Strive.Tracking.Application.UnitTests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.Tracking.Application.UnitTests.Trackers;

public class TrackerWriterTests : IDisposable
{
    private readonly TrackingDatabaseFixture _database = new();

    [Fact]
    public async Task A_new_tracker_is_readable_with_its_counts()
    {
        var id = await CreateAsync(new NewTracker("Coffee", "  cups per day  "));

        await using var context = _database.NewContext();
        var trackers = await new TrackerReader(context).GetTrackersAsync();

        trackers.Should().ContainSingle();
        trackers[0].Id.Should().Be(id);
        trackers[0].Name.Should().Be("Coffee");
        trackers[0].Description.Should().Be("cups per day");
        trackers[0].EventCount.Should().Be(0);
        trackers[0].LastEventUtc.Should().BeNull();
    }

    [Fact]
    public async Task A_name_that_differs_only_in_casing_is_rejected()
    {
        await CreateAsync(new NewTracker("Coffee"));

        var create = async () => await CreateAsync(new NewTracker("COFFEE"));

        await create.Should().ThrowAsync<TrackingException>();
    }

    [Fact]
    public async Task Renaming_a_tracker_to_its_own_name_is_allowed()
    {
        var id = await CreateAsync(new NewTracker("Coffee"));

        await using var context = _database.NewContext();
        var writer = new TrackerWriter(context, TimeProvider.System);

        await writer.UpdateTrackerAsync(id, new TrackerEdit("Coffee", "still coffee"));

        var tracker = await new TrackerReader(_database.NewContext()).GetTrackerAsync(id);
        tracker!.Description.Should().Be("still coffee");
    }

    [Fact]
    public async Task A_blank_name_is_rejected()
    {
        var create = async () => await CreateAsync(new NewTracker("   "));

        await create.Should().ThrowAsync<TrackingException>();
    }

    [Fact]
    public async Task Fields_are_ordered_by_the_sequence_they_were_added_in()
    {
        var id = await CreateAsync(new NewTracker("Gym"));

        await using (var context = _database.NewContext())
        {
            var writer = new TrackerWriter(context, TimeProvider.System);

            await writer.AddFieldAsync(id, new NewField("Duration", TrackerFieldType.Number, "min"));
            await writer.AddFieldAsync(id, new NewField("Mood", TrackerFieldType.Text));
        }

        await using var reading = _database.NewContext();
        var tracker = await new TrackerReader(reading).GetTrackerAsync(id);

        tracker!.Fields.Select(field => field.Name).Should().Equal("Duration", "Mood");
        tracker.Fields[0].SortOrder.Should().Be(0);
        tracker.Fields[1].SortOrder.Should().Be(1);
        tracker.Fields[0].Unit.Should().Be("min");
    }

    [Fact]
    public async Task Two_fields_of_one_tracker_cannot_share_a_name()
    {
        var id = await CreateAsync(new NewTracker("Gym"));

        await using var context = _database.NewContext();
        var writer = new TrackerWriter(context, TimeProvider.System);
        await writer.AddFieldAsync(id, new NewField("Duration", TrackerFieldType.Number));

        var add = async () =>
            await writer.AddFieldAsync(id, new NewField("duration", TrackerFieldType.Text));

        await add.Should().ThrowAsync<TrackingException>();
    }

    [Fact]
    public async Task Deleting_a_tracker_takes_its_fields_events_and_values_with_it()
    {
        var id = await CreateAsync(new NewTracker("Coffee"));

        await using (var context = _database.NewContext())
        {
            var writer = new TrackerWriter(context, TimeProvider.System);
            var fieldId = await writer.AddFieldAsync(
                id,
                new NewField("Cups", TrackerFieldType.Number)
            );

            var recorder = new EventRecorder(context, TimeProvider.System);
            await recorder.RecordAsync(
                id,
                new NewEvent(DateTimeOffset.UtcNow, Values: [new NewEventValue(fieldId, 2m)])
            );
        }

        await using (var context = _database.NewContext())
            await new TrackerWriter(context, TimeProvider.System).DeleteTrackerAsync(id);

        await using var verifying = _database.NewContext();
        (await verifying.Trackers.CountAsync()).Should().Be(0);
        (await verifying.TrackerFields.CountAsync()).Should().Be(0);
        (await verifying.TrackerEvents.CountAsync()).Should().Be(0);
        (await verifying.TrackerEventValues.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Deleting_a_field_leaves_the_events_but_drops_their_values()
    {
        var id = await CreateAsync(new NewTracker("Coffee"));

        await using (var context = _database.NewContext())
        {
            var writer = new TrackerWriter(context, TimeProvider.System);
            var fieldId = await writer.AddFieldAsync(
                id,
                new NewField("Cups", TrackerFieldType.Number)
            );

            var recorder = new EventRecorder(context, TimeProvider.System);
            await recorder.RecordAsync(
                id,
                new NewEvent(DateTimeOffset.UtcNow, Values: [new NewEventValue(fieldId, 2m)])
            );

            await writer.DeleteFieldAsync(fieldId);
        }

        await using var verifying = _database.NewContext();
        (await verifying.TrackerEvents.CountAsync()).Should().Be(1);
        (await verifying.TrackerEventValues.CountAsync()).Should().Be(0);
    }

    private async Task<Guid> CreateAsync(NewTracker tracker)
    {
        await using var context = _database.NewContext();
        return await new TrackerWriter(context, TimeProvider.System).CreateTrackerAsync(tracker);
    }

    public void Dispose() => _database.Dispose();
}
