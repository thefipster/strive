namespace TheFipster.ActivityAggregator.Web.Layout;

public partial class MainLayout
{
    private int year = DateTime.Now.Year;
    private string pageName = "";

    protected override void OnParametersSet()
    {
        if (Navigation != null && Navigation.Uri.Contains("calendar/year"))
        {
            pageName = "yearlyCalendar";
            var url = Navigation.ToAbsoluteUri(Navigation.Uri);
            var yearValue = url.ToString().Split("/").Last();
            if (int.TryParse(yearValue, out int parsedYear))
                year = parsedYear;
        }
        else
        {
            pageName = "";
        }

        base.OnParametersSet();
    }
}
