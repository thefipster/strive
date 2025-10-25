namespace Fip.Strive.Harvester.Application.Core.Config.Models;

public abstract class BaseConfig(string configSectionName)
{
    public string ConfigSectionName { get; } = configSectionName;
}
