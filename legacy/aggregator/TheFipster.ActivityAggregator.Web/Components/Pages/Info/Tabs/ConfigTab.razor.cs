using Microsoft.AspNetCore.Components;

namespace TheFipster.ActivityAggregator.Web.Components.Pages.Info.Tabs;

public partial class ConfigTab : ComponentBase
{
    [Inject]
    public required IConfiguration Configuration { get; set; }

    private Dictionary<string, string> _values = [];

    protected override void OnParametersSet()
    {
        var result = new Dictionary<string, string>();
        AddChildren(Configuration, result, parentPath: null);
        _values = result;
    }

    private static void AddChildren(
        IConfiguration configuration,
        Dictionary<string, string> result,
        string? parentPath
    )
    {
        foreach (var child in configuration.GetChildren())
        {
            var key = parentPath == null ? child.Key : $"{parentPath}:{child.Key}";
            if (child.Value == null)
            {
                // Has nested children → recurse
                AddChildren(child, result, key);
            }
            else
            {
                result[key] = child.Value;
            }
        }
    }
}
