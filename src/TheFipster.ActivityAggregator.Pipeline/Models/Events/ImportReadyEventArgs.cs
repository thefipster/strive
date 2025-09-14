using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Pipeline.Models;

public class ImportReadyEventArgs(ClassificationIndex index) : EventArgs
{
    public ClassificationIndex Index => index;
}
