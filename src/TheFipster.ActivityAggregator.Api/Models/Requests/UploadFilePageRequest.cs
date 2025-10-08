namespace TheFipster.ActivityAggregator.Api.Models.Requests;

public class UploadFilePageRequest
{
    public int Page { get; set; } = 0;
    public int Size { get; set; } = 10;

    public string? Search { get; set; }
}
