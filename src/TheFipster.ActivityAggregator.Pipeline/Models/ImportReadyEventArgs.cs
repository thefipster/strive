using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Pipeline.Models;

public class ImportReadyEventArgs(ImportIndex index) : EventArgs
{
    public ImportIndex Index => index;
}
