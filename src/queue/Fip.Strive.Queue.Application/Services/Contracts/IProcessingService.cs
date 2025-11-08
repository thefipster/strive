namespace Fip.Strive.Queue.Application.Services.Contracts;

public interface IProcessingService
{
    bool IsRunning { get; }

    Task ExecuteAsync(CancellationToken ct);
}
