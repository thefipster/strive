using System.Diagnostics.CodeAnalysis;
using Fip.Strive.Core.Domain.Exceptions;
using Fip.Strive.Harvester.Application.Features.Upload.Services;
using Fip.Strive.Harvester.Application.Features.Upload.Services.Contracts;
using Fip.Strive.Harvester.Application.Features.Upload.Services.Decorators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace Fip.Strive.Harvester.Application.Features.Upload;

[ExcludeFromCodeCoverage]
public static class Registration
{
    public static void AddUploadFeature(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddScoped<IUploadService, UploadService>();
        services.Decorate<IUploadService, UploadValidation>();
        services.Decorate<IUploadService, UploadPublisher>();

        services.AddSingleton<IConnectionFactory>(sp =>
        {
            var rabbit = configuration.GetConnectionString("rabbitmq");

            if (string.IsNullOrWhiteSpace(rabbit))
                throw new ConfigurationException("RabbitMQ connection string is missing.");

            return new ConnectionFactory { Uri = new Uri(rabbit) };
        });
    }
}
