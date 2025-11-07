namespace Fip.Strive.Queue.Application.Components.Contracts;

public interface IQueueReporter
{
    Task RunAsync(CancellationToken ct);
}
