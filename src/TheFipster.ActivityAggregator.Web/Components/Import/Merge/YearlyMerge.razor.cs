using Microsoft.AspNetCore.Components;
using TheFipster.ActivityAggregator.Web.Services;

namespace TheFipster.ActivityAggregator.Web.Components.Import.Merge;

public partial class YearlyMerge : ComponentBase
{
    private Dictionary<int, int[]> inventory = new();

    [Inject]
    public ApiService? Api { get; set; }

    [Parameter]
    public EventCallback<int> YearSelected { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        if (Api != null)
            inventory = (await Api.GetYearlyInventoryAsync())
                .OrderBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.Value);

        await base.OnParametersSetAsync();
    }

    private async Task OnYearClick(int year)
    {
        await YearSelected.InvokeAsync(year);
    }
}
