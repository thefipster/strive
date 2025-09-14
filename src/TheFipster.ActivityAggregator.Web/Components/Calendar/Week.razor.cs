using Microsoft.AspNetCore.Components;

namespace Fipster.TrackMe.Web.Components.Calendar
{
    public partial class Week
    {
        [Parameter]
        public DateTime Start { get; set; }

        [Parameter]
        public int Index { get; set; }

        [Parameter]
        public Dictionary<DateTime, int>? Data { get; set; }

        public int GetState(DateTime day)
        {
            if (Data != null && Data.TryGetValue(day, out var state))
                return state;

            return 0;
        }
    }
}
