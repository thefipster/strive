using TheFipster.ActivityAggregator.Pipeline.Extensions;

namespace TheFipster.ActivityAggregator.Pipeline.Models;

public class ProgressCounters
{
    public Counter In { get; private set; } = new();
    public Counter Out { get; private set; } = new();

    public Counter Done { get; private set; } = new();
    public Counter Skip { get; private set; } = new();
}
