namespace Fip.Strive.Core.Application.Features.Config.Models;

public abstract class BaseConfig(string configSectionName)
{
    public string ConfigSectionName { get; } = configSectionName;
}
