using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Harvester.Application.Infrastructure.Indexing;

public static class ConfigureIndexing
{
    public static ContextExtension WithIndexingContext(this ContextExtension extension)
    {
        extension.Services.AddDbContext<IndexContext>(options =>
            options.UseNpgsql(extension.ConnectionString)
        );

        return extension;
    }
}
