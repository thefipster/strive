using Microsoft.AspNetCore.Components;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Web.Components.YearCalendar;

public partial class MonthColumn : ComponentBase
{
    [Inject]
    public NavigationManager? Navigation { get; set; }

    [Parameter]
    public DateTime Month { get; set; } = new(DateTime.Now.Year, DateTime.Now.Month, 1);

    [Parameter]
    public List<InventoryIndex> Inventory { get; set; } = [];

    [Parameter]
    public List<DateTime> Batches { get; set; } = [];

    private Dictionary<int, List<InventoryIndex>> _dailyInventory = new();

    protected override Task OnParametersSetAsync()
    {
        var groupedInvetory = Inventory
            .GroupBy(x => x.Timestamp.Day)
            .ToDictionary(x => x.Key, y => y.ToList());

        for (int i = 1; i <= DateTime.DaysInMonth(Month.Year, Month.Month); i++)
        {
            if (!groupedInvetory.ContainsKey(i))
                groupedInvetory.Add(i, []);
        }

        _dailyInventory = groupedInvetory;

        return base.OnParametersSetAsync();
    }

    private void OnDayAvatarClicked(int day)
    {
        if (Navigation == null)
            return;

        var date = new DateTime(Month.Year, Month.Month, day);
        Navigation.NavigateTo($"batch?date={date:yyyy-MM-dd}");
    }
}
