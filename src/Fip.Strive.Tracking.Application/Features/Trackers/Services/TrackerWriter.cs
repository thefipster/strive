using Fip.Strive.Tracking.Application.Features.Trackers.Models;
using Fip.Strive.Tracking.Application.Features.Trackers.Services.Contracts;
using Fip.Strive.Tracking.Application.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.Tracking.Application.Features.Trackers.Services;

public sealed class TrackerWriter(TrackingContext context, TimeProvider clock) : ITrackerWriter
{
    public async Task<Guid> CreateTrackerAsync(
        NewTracker tracker,
        CancellationToken cancellationToken = default
    )
    {
        var name = CleanName(tracker.Name);
        await GuardTrackerNameIsFreeAsync(name, null, cancellationToken);

        var entity = new Tracker
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            Description = CleanOptional(
                tracker.Description,
                TrackingLimits.DescriptionLength,
                "Description"
            ),
            CreatedUtc = clock.GetUtcNow(),
        };

        context.Trackers.Add(entity);
        await SaveAsync($"A tracker called '{name}' already exists.", cancellationToken);

        return entity.Id;
    }

    public async Task UpdateTrackerAsync(
        Guid trackerId,
        TrackerEdit edit,
        CancellationToken cancellationToken = default
    )
    {
        var tracker =
            await context.Trackers.FirstOrDefaultAsync(
                candidate => candidate.Id == trackerId,
                cancellationToken
            ) ?? throw new TrackingException("That tracker no longer exists.");

        var name = CleanName(edit.Name);
        await GuardTrackerNameIsFreeAsync(name, trackerId, cancellationToken);

        tracker.Name = name;
        tracker.Description = CleanOptional(
            edit.Description,
            TrackingLimits.DescriptionLength,
            "Description"
        );

        await SaveAsync($"A tracker called '{name}' already exists.", cancellationToken);
    }

    public async Task DeleteTrackerAsync(
        Guid trackerId,
        CancellationToken cancellationToken = default
    )
    {
        // Fields, events and values go with it through the foreign keys' cascade, which the SQLite
        // provider has switched on — no need to pull the history into memory just to delete it.
        var deleted = await context
            .Trackers.Where(tracker => tracker.Id == trackerId)
            .ExecuteDeleteAsync(cancellationToken);

        if (deleted == 0)
            throw new TrackingException("That tracker no longer exists.");
    }

    public async Task<Guid> AddFieldAsync(
        Guid trackerId,
        NewField field,
        CancellationToken cancellationToken = default
    )
    {
        var exists = await context.Trackers.AnyAsync(
            tracker => tracker.Id == trackerId,
            cancellationToken
        );

        if (!exists)
            throw new TrackingException("That tracker no longer exists.");

        var name = CleanName(field.Name);
        await GuardFieldNameIsFreeAsync(trackerId, name, null, cancellationToken);

        // Cast to nullable so the first field of a tracker starts at 0 rather than failing on an
        // aggregate over nothing.
        var lastOrder = await context
            .TrackerFields.Where(candidate => candidate.TrackerId == trackerId)
            .MaxAsync(candidate => (int?)candidate.SortOrder, cancellationToken);

        var entity = new TrackerField
        {
            Id = Guid.CreateVersion7(),
            TrackerId = trackerId,
            Name = name,
            Type = field.Type,
            Unit = CleanOptional(field.Unit, TrackingLimits.UnitLength, "Unit"),
            IsRequired = field.IsRequired,
            SortOrder = (lastOrder ?? -1) + 1,
        };

        context.TrackerFields.Add(entity);
        await SaveAsync($"This tracker already has a field called '{name}'.", cancellationToken);

        return entity.Id;
    }

    public async Task UpdateFieldAsync(
        Guid fieldId,
        FieldEdit edit,
        CancellationToken cancellationToken = default
    )
    {
        var field =
            await context.TrackerFields.FirstOrDefaultAsync(
                candidate => candidate.Id == fieldId,
                cancellationToken
            ) ?? throw new TrackingException("That field no longer exists.");

        var name = CleanName(edit.Name);
        await GuardFieldNameIsFreeAsync(field.TrackerId, name, fieldId, cancellationToken);

        field.Name = name;
        field.Unit = CleanOptional(edit.Unit, TrackingLimits.UnitLength, "Unit");
        field.IsRequired = edit.IsRequired;

        await SaveAsync($"This tracker already has a field called '{name}'.", cancellationToken);
    }

    public async Task DeleteFieldAsync(Guid fieldId, CancellationToken cancellationToken = default)
    {
        var deleted = await context
            .TrackerFields.Where(field => field.Id == fieldId)
            .ExecuteDeleteAsync(cancellationToken);

        if (deleted == 0)
            throw new TrackingException("That field no longer exists.");
    }

    /// <summary>
    /// Saves, turning a unique-index violation into the same <see cref="TrackingException"/> the
    /// pre-flight guards throw.
    /// </summary>
    /// <remarks>
    /// The guards query and then insert, so two tabs submitting at once can both pass the check and
    /// leave the second insert to the index. The outcome was always *safe* — the index is what
    /// actually enforces uniqueness — but the loser got a raw <c>DbUpdateException</c>, and the
    /// pages catch only <see cref="TrackingException"/>, so a duplicate name surfaced as an
    /// unhandled error instead of the snackbar it deserves.
    /// </remarks>
    private async Task SaveAsync(string conflictMessage, CancellationToken cancellationToken)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            throw new TrackingException(conflictMessage, exception);
        }
    }

    /// <summary>
    /// SQLite reports a broken UNIQUE index as extended error 2067. Matching the extended code
    /// rather than the plain <c>SQLITE_CONSTRAINT</c> keeps a foreign-key or NOT NULL failure —
    /// which would be a bug here, not a user error — reported as the bug it is.
    /// </summary>
    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is SqliteException { SqliteExtendedErrorCode: 2067 };

    private async Task GuardTrackerNameIsFreeAsync(
        string name,
        Guid? ignoring,
        CancellationToken cancellationToken
    )
    {
        // The unique index is case sensitive, so "Coffee" and "coffee" would both fit through it
        // and read as the same tracker to anyone looking at the list.
        var lowered = name.ToLowerInvariant();

        var query = context.Trackers.Where(tracker => tracker.Name.ToLower() == lowered);

        // Renaming a tracker to what it is already called must not collide with itself.
        if (ignoring is { } excluded)
            query = query.Where(tracker => tracker.Id != excluded);

        if (await query.AnyAsync(cancellationToken))
            throw new TrackingException($"A tracker called '{name}' already exists.");
    }

    private async Task GuardFieldNameIsFreeAsync(
        Guid trackerId,
        string name,
        Guid? ignoring,
        CancellationToken cancellationToken
    )
    {
        var lowered = name.ToLowerInvariant();

        var query = context.TrackerFields.Where(field =>
            field.TrackerId == trackerId && field.Name.ToLower() == lowered
        );

        if (ignoring is { } excluded)
            query = query.Where(field => field.Id != excluded);

        if (await query.AnyAsync(cancellationToken))
            throw new TrackingException($"This tracker already has a field called '{name}'.");
    }

    private static string CleanName(string? value)
    {
        var name = value?.Trim();

        if (string.IsNullOrEmpty(name))
            throw new TrackingException("A name is required.");

        if (name.Length > TrackingLimits.NameLength)
            throw new TrackingException(
                $"Names are limited to {TrackingLimits.NameLength} characters."
            );

        return name;
    }

    private static string? CleanOptional(string? value, int maxLength, string label)
    {
        var text = value?.Trim();

        if (string.IsNullOrEmpty(text))
            return null;

        if (text.Length > maxLength)
            throw new TrackingException($"{label} is limited to {maxLength} characters.");

        return text;
    }
}
