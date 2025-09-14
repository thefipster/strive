namespace TheFipster.ActivityAggregator.Pipeline.Models;

public class ProgressReportEventArgs(
    string stage,
    int queue,
    int total,
    int processed,
    int errors,
    int emitted
) : EventArgs
{
    public string Stage => stage;
    public int Total => total;
    public int Queue => queue;
    public int Processed => processed;
    public int Errors => errors;
    public int Emitted => emitted;
}
