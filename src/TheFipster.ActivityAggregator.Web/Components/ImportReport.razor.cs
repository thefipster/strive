using Microsoft.AspNetCore.Components;
using TheFipster.ActivityAggregator.Domain.Models;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Web.Components;

public partial class ImportReport
{
    [Parameter]
    public MasterIndex? Index { get; set; }

    [Parameter]
    public UnifiedRecord? Activity { get; set; }
}
