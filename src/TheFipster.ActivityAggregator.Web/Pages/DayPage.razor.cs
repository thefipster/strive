using TheFipster.ActivityAggregator.Domain.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Fipster.TrackMe.Web.Pages
{
    public partial class DayPage
    {
        [Parameter]
        public string? DateValue { get; set; }

        public DateTime? Date { get; set; }

        public IEnumerable<FileExtraction>? Data { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            if (!string.IsNullOrWhiteSpace(DateValue))
            {
                Date = DateTime.Parse(DateValue);
                Data = await Api.GetDayAsync(Date.Value);
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
