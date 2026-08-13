using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Fip.Strive.Tracking.Application.Infrastructure.Configurations;

public static class TrackingConversions
{
    /// <summary>
    /// Stores a <see cref="DateTimeOffset"/> as fixed-width ISO-8601 text. SQLite has no date type
    /// and the provider's own <see cref="DateTimeOffset"/> mapping is unordered — it throws on
    /// ORDER BY and on MAX — so every timestamp column in this model goes through here instead.
    /// Ordering the text is ordering the instants only because everything is normalised to UTC
    /// before it is written; see <c>EventRecorder</c>.
    /// </summary>
    public static readonly DateTimeOffsetToStringConverter UtcText = new();
}
