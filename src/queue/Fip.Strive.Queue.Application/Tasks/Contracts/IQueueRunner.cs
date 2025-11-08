namespace Fip.Strive.Queue.Application.Tasks.Contracts;

public interface IQueueRunner
{
    public Guid Id { get; }
    public bool IsRunning { get; }
    Task RunAsync(CancellationToken ct);
}
