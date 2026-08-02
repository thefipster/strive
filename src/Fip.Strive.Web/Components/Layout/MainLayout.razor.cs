using MudBlazor;

namespace Fip.Strive.Web.Components.Layout;

public partial class MainLayout
{
    private bool _drawerOpen = true;
    private bool _isDarkMode = true;
    private MudThemeProvider? _mudThemeProvider;

    private readonly MudTheme _striveTheme = new()
    {
        Typography = new Typography
        {
            H1 = new H1Typography { FontSize = "3rem" },
            H2 = new H2Typography { FontSize = "2rem" },
            H3 = new H3Typography { FontSize = "1.6rem" },
            H4 = new H4Typography { FontSize = "1.3rem" },
            Body1 = new Body1Typography { FontSize = "1rem" },
        },
    };

    private void ToggleDrawer() => _drawerOpen = !_drawerOpen;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && _mudThemeProvider is not null)
        {
            _isDarkMode = await _mudThemeProvider.GetSystemDarkModeAsync();
            await _mudThemeProvider.WatchSystemDarkModeAsync(OnSystemDarkModeChanged);
            StateHasChanged();
        }
    }

    private Task OnSystemDarkModeChanged(bool newValue)
    {
        _isDarkMode = newValue;
        StateHasChanged();
        return Task.CompletedTask;
    }
}
