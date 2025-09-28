using Microsoft.AspNetCore.Components;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Web.Services;

namespace TheFipster.ActivityAggregator.Web.Components.Import.Merge;

public partial class InventoryYear : ComponentBase
{
    private IEnumerable<DateTime> days = [];
    private IEnumerable<DateTime> sessions = [];

    [Inject]
    public ApiService? Api { get; set; }

    [Parameter]
    public int? Year { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        if (Api != null && Year != null)
        {
            var inventory = (await Api.GetInventoryByYearAsync(Year.Value)).ToArray();

            days = inventory
                .Where(x => x.IsDay)
                .OrderBy(x => x.Timestamp)
                .Select(x => x.Timestamp)
                .Distinct();

            sessions = inventory
                .Where(x => !x.IsDay)
                .OrderBy(x => x.Timestamp)
                .Select(x => x.Timestamp)
                .Distinct();
        }

        await InvokeAsync(StateHasChanged);
        await base.OnParametersSetAsync();
    }
}
