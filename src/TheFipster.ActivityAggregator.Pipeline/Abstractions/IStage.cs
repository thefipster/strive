using TheFipster.ActivityAggregator.Pipeline.Models;
using TheFipster.ActivityAggregator.Pipeline.Models.Events;

namespace TheFipster.ActivityAggregator.Pipeline.Abstractions;

public interface IStage<in TInput, TOutput> : IStage
{
    void Enqueue(TInput import);
    event EventHandler<ResultReportEventArgs<TOutput>>? ReportResult;

    Task ExecuteAsync(CancellationToken token);
}

public interface IStage
{
    int Version { get; }
    string Name { get; }
    int Order { get; }

    ProgressCounters Counters { get; }
}
