using TheFipster.ActivityAggregator.Pipeline.Models;
using TheFipster.ActivityAggregator.Pipeline.Models.Events;

namespace TheFipster.ActivityAggregator.Pipeline.Abstractions;

public interface IStage<in TInput, TOutput> : IStage
{
    void Enqueue(TInput input);
    event EventHandler<ResultReportEventArgs<TOutput>>? ReportResult;

    Task ExecuteAsync(CancellationToken ct);
}

public interface IStage
{
    int Version { get; }
    int Order { get; }
    ProgressCounters Counters { get; }
}
