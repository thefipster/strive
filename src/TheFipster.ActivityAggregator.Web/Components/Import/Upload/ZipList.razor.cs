using Microsoft.AspNetCore.Components;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Web.Services;

namespace TheFipster.ActivityAggregator.Web.Components.Import.Upload;

public partial class ZipList : ComponentBase
{
    [Parameter]
    public IEnumerable<ZipIndex> Zips { get; set; } = [];

    [Parameter] public bool IsLoading { get; set; } = false;
}
