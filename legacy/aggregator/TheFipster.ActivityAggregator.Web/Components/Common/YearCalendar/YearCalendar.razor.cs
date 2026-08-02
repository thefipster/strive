using System.Reactive.Linq;
using Microsoft.AspNetCore.Components;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Web.Components.Common.YearCalendar;

public partial class YearCalendar : ComponentBase
{
    [Parameter]
    public int Year { get; set; } = DateTime.Now.Year;

    [Parameter]
    public IEnumerable<InventoryIndex> Inventory { get; set; } = [];

    [Parameter]
    public IEnumerable<DateTime> Batches { get; set; } = [];

    private Dictionary<DateTime, List<InventoryIndex>> _monthlyInventory = new();

    private Dictionary<DateTime, List<DateTime>> _monthyBatches = new();

    protected override Task OnParametersSetAsync()
    {
        var groupedInvetory = Inventory
            .GroupBy(x => x.Timestamp.Month)
            .ToDictionary(x => new DateTime(x.First().Timestamp.Year, x.Key, 1), y => y.ToList());

        for (int i = 1; i <= 12; i++)
        {
            var month = new DateTime(Year, i, 1);
            if (!groupedInvetory.ContainsKey(month))
                groupedInvetory.Add(month, []);

            var batches = (Batches ?? []).Where(x => x.Month == i).ToList();
            _monthyBatches[month] = batches;
        }

        _monthlyInventory = groupedInvetory;

        return base.OnParametersSetAsync();
    }
}
