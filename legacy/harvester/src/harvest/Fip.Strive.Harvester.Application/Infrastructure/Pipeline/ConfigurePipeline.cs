using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Harvester.Application.Infrastructure.Pipeline;

public static class ConfigurePipeline
{
    public static ContextExtension WithPipelineContext(this ContextExtension extension)
    {
        extension.Services.AddDbContext<PipelineContext>(options =>
            options.UseNpgsql(extension.ConnectionString)
        );

        return extension;
    }
}
