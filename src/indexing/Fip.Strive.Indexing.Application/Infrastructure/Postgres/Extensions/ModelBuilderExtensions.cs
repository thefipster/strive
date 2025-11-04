using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.Indexing.Application.Infrastructure.Postgres.Extensions;

public static class ModelBuilderExtensions
{
    public static ModelBuilder ApplyConfigurationsFromNamespace(
        this ModelBuilder modelBuilder,
        string namespaceName
    )
    {
        var assembly = Assembly.GetExecutingAssembly();

        var configurationTypes = assembly
            .GetTypes()
            .Where(t =>
                t.Namespace == namespaceName
                && t.GetInterfaces()
                    .Any(i =>
                        i.IsGenericType
                        && i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>)
                    )
                && !t.IsAbstract
                && !t.IsInterface
            );

        foreach (var configurationType in configurationTypes)
        {
            dynamic configuration = Activator.CreateInstance(configurationType)!;
            modelBuilder.ApplyConfiguration(configuration);
        }

        return modelBuilder;
    }
}
