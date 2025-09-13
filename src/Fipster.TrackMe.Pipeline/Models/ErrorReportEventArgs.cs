namespace Fipster.TrackMe.Pipeline.Models;

public class ErrorReportEventArgs(string stage, Exception ex) : EventArgs
{
    public string Stage => stage;
    public Exception Exception => ex;
    public string Message => $"{stage, 16}: {ex.Message}";
}
