namespace Fip.Strive.Application.UnitTests.Fixtures;

/// <summary>A clock the test moves by hand, so throttling can be asserted without sleeping.</summary>
public sealed class StubClock(DateTimeOffset now) : TimeProvider
{
    public DateTimeOffset Now { get; set; } = now;

    public override DateTimeOffset GetUtcNow() => Now;

    public DateTimeOffset Advance(TimeSpan step) => Now += step;
}
