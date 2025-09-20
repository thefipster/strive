using System.Collections.Concurrent;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Pipeline.Models;
using TheFipster.ActivityAggregator.Pipeline.Models.Events;

namespace TheFipster.ActivityAggregator.Pipeline.Stages;

public class Stage<TInput, TOutput>
{
    public ProgressCounters Counters { get; } = new();

    protected readonly ConcurrentQueue<TInput> queue = new();

    public void Enqueue(TInput import)
    {
        queue.Enqueue(import);
        Counters.In.Increment();
    }
}
