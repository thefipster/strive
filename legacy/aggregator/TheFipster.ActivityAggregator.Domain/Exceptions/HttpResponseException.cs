namespace TheFipster.ActivityAggregator.Domain.Exceptions;

public class HttpResponseException : Exception
{
    public HttpResponseException() { }

    public HttpResponseException(int status, string reason)
        : base($"{status}: {reason}")
    {
        StatusCode = status;
        ReasonPhrase = reason;
    }

    public HttpResponseException(int status, string title, string reason)
        : base($"{status} - {title}: {reason}")
    {
        StatusCode = status;
        Title = title;
        ReasonPhrase = reason;
    }

    public int StatusCode { get; set; } = 500;
    public string Title { get; set; } = "Error";
    public string ReasonPhrase { get; set; } = "Internal server error.";
}
