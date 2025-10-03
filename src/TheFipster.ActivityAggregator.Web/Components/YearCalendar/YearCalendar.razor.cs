using Microsoft.AspNetCore.Components;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Web.Components.YearCalendar;

public partial class YearCalendar : ComponentBase
{
    [Parameter]
    public int Year { get; set; } = DateTime.Now.Year;

    [Parameter]
    public IEnumerable<InventoryIndex> Inventory { get; set; } = [];

    private Dictionary<DateTime, List<InventoryIndex>> monthlyInventory = new();

    protected override Task OnParametersSetAsync()
    {
        var groupedInvetory = Inventory
            .GroupBy(x => x.Timestamp.Month)
            .ToDictionary(x => new DateTime(Year, x.Key, 1), y => y.ToList());

        for (int i = 1; i <= 12; i++)
        {
            var month = new DateTime(Year, i, 1);
            if (!groupedInvetory.ContainsKey(month))
                groupedInvetory.Add(month, []);
        }

        monthlyInventory = groupedInvetory;

        return base.OnParametersSetAsync();
    }
}
