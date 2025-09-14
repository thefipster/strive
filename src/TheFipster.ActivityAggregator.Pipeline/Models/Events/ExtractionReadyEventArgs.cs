using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Pipeline.Models;

public class ExtractionReadyEventArgs(TransformIndex index) : EventArgs
{
    public TransformIndex Index => index;
}
