namespace Fip.Strive.Tracking.Application.UnitTests.Fixtures;

/// <summary>
/// A clock the test moves by hand. Anything asserting on <c>RecordedUtc</c> needs one — the real
/// one would stamp several events in the same millisecond.
/// </summary>
public sealed class StubClock(DateTimeOffset now) : TimeProvider
{
    public DateTimeOffset Now { get; set; } = now;

    public override DateTimeOffset GetUtcNow() => Now;

    public DateTimeOffset Advance(TimeSpan step) => Now += step;
}
