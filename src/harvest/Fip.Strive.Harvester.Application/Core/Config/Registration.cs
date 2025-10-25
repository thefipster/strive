using System.Reflection;
using Fip.Strive.Core.Domain.Exceptions;
using Fip.Strive.Harvester.Application.Core.Config.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Harvester.Application.Core.Config;

public static class Registration
{
    public static void AddConfigs(this IServiceCollection services, IConfiguration configuration)
    {
        var baseType = typeof(BaseConfig);

        var configTypes = Assembly
            .GetAssembly(baseType)
            ?.GetTypes()
            .Where(t => !t.IsAbstract && baseType.IsAssignableFrom(t))
            .ToList();

        if (configTypes == null || !configTypes.Any())
            throw new ConfigurationException("Couldn't find any config types.");

        foreach (var type in configTypes)
        {
            if (Activator.CreateInstance(type) is BaseConfig instance)
            {
                var method = typeof(OptionsConfigurationServiceCollectionExtensions)
                    .GetMethods(BindingFlags.Static | BindingFlags.Public)
                    .First(m =>
                        m.Name == nameof(OptionsConfigurationServiceCollectionExtensions.Configure)
                        && m.GetGenericArguments().Length == 1
                        && m.GetParameters().Length == 2
                    );

                var generic = method.MakeGenericMethod(type);
                var section = configuration.GetSection(instance.ConfigSectionName);
                generic.Invoke(null, new object[] { services, section });
            }
        }
    }
}
