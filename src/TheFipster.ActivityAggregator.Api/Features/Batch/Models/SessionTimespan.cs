using TheFipster.ActivityAggregator.Domain.Models.Files;

namespace TheFipster.ActivityAggregator.Api.Features.Batch.Models;

public class SessionTimespan
{
    public DateTime Timestamp { get; set; }
    public DateTime? Start { get; set; }
    public DateTime? End { get; set; }

    public static SessionTimespan New(MergedFile merge)
    {
        DateTime? start = null;
        DateTime? end = null;

        if (merge.Samples.Any())
        {
            start = merge.Samples.Min(x => x.Start);
            end = merge.Samples.Max(x => x.End);
        }

        return new SessionTimespan
        {
            Timestamp = merge.Timestamp,
            Start = start,
            End = end,
        };
    }
}
