using System.Collections.Concurrent;

namespace TheFipster.ActivityAggregator.Pipeline.Models;

public class PipelineState
{
    public ConcurrentBag<string> FinishedStages { get; private set; } = new();
}
