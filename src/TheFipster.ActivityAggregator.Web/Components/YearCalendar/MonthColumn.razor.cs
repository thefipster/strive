using Microsoft.AspNetCore.Components;
using TheFipster.ActivityAggregator.Domain.Models;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Web.Components.YearCalendar;

public partial class MonthColumn : ComponentBase
{
    [Parameter]
    public DateTime Month { get; set; } = new(DateTime.Now.Year, DateTime.Now.Month, 1);

    [Parameter]
    public List<InventoryIndex> Inventory { get; set; } = [];

    private Dictionary<int, List<InventoryIndex>> dailyInventory = new();

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

        dailyInventory = groupedInvetory;

        return base.OnParametersSetAsync();
    }
}
