using Microsoft.AspNetCore.Components;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Web.Services;

namespace TheFipster.ActivityAggregator.Web.Components.Import.Assimilate;

public partial class AssimilatedFiles : ComponentBase
{
    private bool isLoading = true;
    private IEnumerable<AssimilaterIndex> indexes = [];

    [Inject]
    public ApiService? Api { get; set; }

    [Parameter]
    public string? Hash { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        isLoading = true;

        if (Api != null && Hash != null)
        {
            indexes = (await Api.GetAssimilateIndexes(Hash)).OrderByDescending(x => x.IndexedAt);
            isLoading = false;
        }

        await base.OnParametersSetAsync();
    }
}
