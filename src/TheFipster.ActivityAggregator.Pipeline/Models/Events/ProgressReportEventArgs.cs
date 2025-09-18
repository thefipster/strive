namespace TheFipster.ActivityAggregator.Pipeline.Models;

public class ProgressReportEventArgs(
    string stage,
    int order,
    int total,
    int processed,
    int errors,
    int emitted
) : EventArgs
{
    public string Stage => stage;
    public int Order => order;
    public int Total => total;
    public int Processed => processed;
    public int Errors => errors;
    public int Emitted => emitted;
}
