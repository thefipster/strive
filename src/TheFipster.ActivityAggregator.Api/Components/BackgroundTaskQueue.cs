using System.Threading.Channels;
using TheFipster.ActivityAggregator.Api.Abtraction;

namespace TheFipster.ActivityAggregator.Api.Components;

public class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly Channel<Func<CancellationToken, Task>> queue = Channel.CreateUnbounded<
        Func<CancellationToken, Task>
    >();

    public void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem) =>
        queue.Writer.TryWrite(workItem);

    public async Task<Func<CancellationToken, Task>> DequeueAsync(
        CancellationToken cancellationToken
    ) => await queue.Reader.ReadAsync(cancellationToken);
}
