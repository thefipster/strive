using Fip.Strive.Queue.Application.Services.Contracts;
using Fip.Strive.Queue.Application.Tasks.Contracts;
using Fip.Strive.Queue.Domain.Models;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Queue.Application.Services;

public class ProcessingService(
    IOptions<QueueConfig> config,
    ITaskFactory workerFactory,
    IQueueService queue
) : IProcessingService
{
    private IEnumerable<IQueueRunner> _runners = [];
    private IEnumerable<IQueueReporter> _reporters = [];

    public bool IsRunning => _runners.Any(x => x.IsRunning);

    public async Task ExecuteAsync(CancellationToken ct)
    {
        await EnsureQueueIsReady(ct);

        _runners = CreateRunnerInstances();
        _reporters = CreateReporterInstances();

        await TaskExecution(ct);
    }

    private async Task EnsureQueueIsReady(CancellationToken ct)
    {
        while (queue.IsReady == false)
            await Task.Delay(100, ct);
    }

    private IEnumerable<IQueueRunner> CreateRunnerInstances()
    {
        for (int i = 0; i < config.Value.MaxDegreeOfParallelism; i++)
            yield return workerFactory.CreateRunner();
    }

    private IEnumerable<IQueueReporter> CreateReporterInstances()
    {
        yield return workerFactory.CreateReporter();
    }

    private async Task TaskExecution(CancellationToken ct)
    {
        var tasks = Enumerable
            .Empty<Task>()
            .Concat(_runners.Select(x => x.RunAsync(ct)))
            .Concat(_reporters.Select(x => x.RunAsync(ct)));

        await Task.WhenAll(tasks);
    }
}
