namespace Fip.Strive.Harvester.Application.Core.Queue.Components.Contracts;

public interface IQueueRunner
{
    public bool IsRunning { get; }
    Task RunAsync(int workerId, CancellationToken ct);
}
