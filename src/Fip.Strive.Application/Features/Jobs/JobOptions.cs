namespace Fip.Strive.Application.Features.Jobs;

public sealed class JobOptions
{
    public const string SectionName = "Jobs";

    /// <summary>
    /// Whether the runner starts. Off in tests that boot the host but drive jobs themselves, so a
    /// background runner cannot race the thing under test.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Concurrent workers. Defaults to the host's processor count capped at eight — the target box
    /// has eight threads and nothing here benefits from oversubscribing them.
    /// </summary>
    public int Parallelism { get; set; } = Math.Min(Environment.ProcessorCount, 8);

    /// <summary>
    /// How long the pump waits for a signal before looking anyway. The safety net that turns a lost
    /// signal into latency rather than a stuck queue.
    /// </summary>
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Floor between persisted progress writes. Without it a forty-thousand-file unpack would cost
    /// forty thousand UPDATEs.
    /// </summary>
    public TimeSpan ProgressInterval { get; set; } = TimeSpan.FromMilliseconds(500);
}
