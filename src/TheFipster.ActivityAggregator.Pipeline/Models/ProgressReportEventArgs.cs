namespace TheFipster.ActivityAggregator.Pipeline.Models;

public class ProgressReportEventArgs(string stage, int total, int processed, int errors) : EventArgs
{
    public string Stage => stage;
    public int Total => total;
    public int Processed => processed;
    public int Errors => errors;
}
