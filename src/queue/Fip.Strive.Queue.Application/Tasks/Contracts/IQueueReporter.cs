namespace Fip.Strive.Queue.Application.Tasks.Contracts;

public interface IQueueReporter
{
    Task RunAsync(CancellationToken ct);
}
