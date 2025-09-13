using Fipster.TrackMe.Web.Views;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Fipster.TrackMe.Web.Components.Calendar
{
    public partial class Calendar
    {
        [Parameter]
        public string? DateValue { get; set; }
        public bool DialogOpen { get; set; }
        public CalendarViewmodel CalendarModel { get; } = new();
        public Dictionary<DateTime, int> Data { get; private set; } = new();

        protected override async Task OnInitializedAsync()
        {
            var date = string.IsNullOrWhiteSpace(DateValue)
                ? DateTime.Today
                : DateTime.Parse(DateValue);

            CalendarModel.SetMonth(date);
            Data = await Api.GetMonthASync(date);

            await base.OnInitializedAsync();
        }

        private void DatePickerChanged(DateTime? date)
        {
            DialogOpen = false;

            if (date.HasValue)
                NavigationManager.NavigateTo(
                    "/calendar/" + date.Value.ToString("yyyy-MM-dd"),
                    true
                );
        }

        public void NextMonth()
        {
            var date = CalendarModel.CurrentMonth.AddMonths(1);
            NavigationManager.NavigateTo("/calendar/" + date.ToString("yyyy-MM-dd"), true);
        }

        public void PrevMonth()
        {
            var date = CalendarModel.CurrentMonth.AddMonths(-1);
            NavigationManager.NavigateTo("/calendar/" + date.ToString("yyyy-MM-dd"), true);
        }

        private void CalendarDateClicked(MouseEventArgs obj) => DialogOpen = true;
    }
}
