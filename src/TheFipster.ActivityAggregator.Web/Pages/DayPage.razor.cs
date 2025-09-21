using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using TheFipster.ActivityAggregator.Domain.Models;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Web.Pages
{
    public partial class DayPage
    {
        [Parameter]
        public string? DateValue { get; set; }

        public DateTime? Date { get; set; }

        public IEnumerable<MasterIndex>? MasterIndexes { get; set; }

        public IEnumerable<UnifiedRecord>? DayRecords { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            if (!string.IsNullOrWhiteSpace(DateValue))
            {
                Date = DateTime.Parse(DateValue);
                MasterIndexes = await Api.GetDayIndexAsync(Date.Value);
                DayRecords = await Api.GetDayActivityAsync(Date.Value);
            }
        }

        private void NavigateToPreviousDay(MouseEventArgs obj)
        {
            if (Date != null)
                NavigationManager.NavigateTo(
                    "/day/" + Date.Value.AddDays(-1).ToString("yyyy-MM-dd"),
                    true
                );
        }

        private void NavigateToNextDay(MouseEventArgs obj)
        {
            if (Date != null)
                NavigationManager.NavigateTo(
                    "/day/" + Date.Value.AddDays(1).ToString("yyyy-MM-dd"),
                    true
                );
        }

        private void MergeClicked()
        {
            if (Date != null)
                NavigationManager.NavigateTo(
                    "/merge/Day/" + Date.Value.ToString("yyyy-MM-dd"),
                    true
                );
        }
    }
}
