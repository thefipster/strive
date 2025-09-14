using System.Collections.Concurrent;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Pipeline.Extensions;

namespace TheFipster.ActivityAggregator.Pipeline.Models;

public class TaskState<TConfig, TPayload>
{
    public TConfig Config { get; set; }
    public PipelineState PipelineState { get; set; }
    public ConcurrentQueue<TPayload> Queue { get; set; }
    public CancellationToken CancellationToken { get; set; }
    public ProgressCounters Counters { get; set; } = new();

    public TaskState(
        TConfig config,
        PipelineState pipelineState,
        ConcurrentQueue<TPayload> queue,
        CancellationToken token
    )
    {
        Config = config;
        PipelineState = pipelineState;
        Queue = queue;
        CancellationToken = token;
    }
}
