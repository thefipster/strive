using Microsoft.AspNetCore.Components;

namespace Fipster.TrackMe.Web.Components.Calendar
{
    public partial class Day
    {
        [Parameter]
        public DateTime Date { get; set; }

        [Parameter]
        public int State { get; set; }
    }
}
