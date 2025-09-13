using Fipster.TrackMe.Domain;
using Fipster.TrackMe.Web.Models;
using Microsoft.AspNetCore.Components;

namespace Fipster.TrackMe.Web.Pages
{
    public partial class MergePage
    {
        [Parameter]
        public string? RangeValue { get; set; }

        [Parameter]
        public string? DateValue { get; set; }

        public MergeViewModel? Model { get; set; }

        protected override async Task OnInitializedAsync()
        {
            if (string.IsNullOrEmpty(DateValue))
                return;

            if (string.IsNullOrEmpty(RangeValue))
                return;

            var date = DateTime.Parse(DateValue);
            var range = Enum.Parse<DateRanges>(RangeValue);

            var files = await Api.GetDayAsync(date);
            var matches = files.Where(x => x.Range == range);

            Model = new MergeViewModel(date, range, matches);

            await base.OnInitializedAsync();
        }
    }
}
