using TheFipster.ActivityAggregator.Api.Features.Info.Services.Contracts;

namespace TheFipster.ActivityAggregator.Api.Features.Info.Services;

public class ConfigurationService(IConfiguration config) : IConfigurationService
{
    public Dictionary<string, string> GetConfig()
    {
        var result = new Dictionary<string, string>();
        AddChildren(config, result, parentPath: null);
        return result;
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
