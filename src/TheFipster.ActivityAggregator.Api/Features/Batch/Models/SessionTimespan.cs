using TheFipster.ActivityAggregator.Domain.Models.Files;

namespace TheFipster.ActivityAggregator.Api.Features.Batch.Models;

public class SessionTimespan
{
    public DateTime Timestamp { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }

    public static SessionTimespan New(MergedFile merge)
    {
        return new SessionTimespan
        {
            Timestamp = merge.Timestamp,
            Start = merge.Samples.Min(x => x.Start),
            End = merge.Samples.Max(x => x.End),
        };
    }
}
