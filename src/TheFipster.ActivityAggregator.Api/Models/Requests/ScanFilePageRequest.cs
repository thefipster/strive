namespace TheFipster.ActivityAggregator.Api.Models.Requests;

public class ScanFilePageRequest
{
    public int Page { get; set; } = 0;
    public int Size { get; set; } = 10;

    public string? Classified { get; set; }
    public string? Range { get; set; }
    public string? Search { get; set; }
}
