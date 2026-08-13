namespace Fip.Strive.Tracking.Application;

/// <summary>
/// Column widths, in one place so the entity configurations and the services that validate against
/// them cannot drift apart. Generous enough that nobody hits them by accident, tight enough that a
/// pasted novel does not end up in the database.
/// </summary>
public static class TrackingLimits
{
    /// <summary>Tracker and field names.</summary>
    public const int NameLength = 80;

    public const int DescriptionLength = 500;

    /// <summary>Unit suffix of a number field — "kg", "min", "€".</summary>
    public const int UnitLength = 20;

    /// <summary>Free-text note on a single event.</summary>
    public const int NoteLength = 1000;

    /// <summary>Value of a text field.</summary>
    public const int TextValueLength = 500;
}
