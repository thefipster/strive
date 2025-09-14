namespace TheFipster.ActivityAggregator.Pipeline.Models.Events;

public class ResultReportEventArgs<T>(T result) : EventArgs
{
    public T Result => result;
}
