using Microsoft.AspNetCore.Components;
using TheFipster.ActivityAggregator.Domain.Models;

namespace TheFipster.ActivityAggregator.Web.Pages
{
    public partial class CalendarPage
    {
        [Parameter]
        public string? Year { get; set; }

        private int year = DateTime.Now.Year;
        private IEnumerable<Inventory> inventory = [];

        protected override async Task OnParametersSetAsync()
        {
            if (!string.IsNullOrWhiteSpace(Year) && int.TryParse(Year, out int yearValue))
                year = yearValue;

            inventory = await Api.GetInvetoryAsync(year);

            await base.OnParametersSetAsync();
        }
    }
}
