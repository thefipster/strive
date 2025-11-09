using System.Diagnostics.CodeAnalysis;
using Fip.Strive.Core.Application.Extensions;
using Fip.Strive.Queue.Domain;
using Fip.Strive.Queue.Storage.Contracts;
using Fip.Strive.Queue.Storage.Memory.Contexts;
using Fip.Strive.Queue.Storage.Memory.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Queue.Storage.Memory;

[ExcludeFromCodeCoverage]
public static class Registration
{
    public static QueueFeatureBuilder WithInMemoryStorage(this QueueFeatureBuilder builder)
    {
        var services = builder.Services;

        services.AddSingleton<MemoryQueueContext>();

        services.AddSingleton<IJobControl, MemoryJobControl>();
        services.AddScoped<IJobReader, MemoryJobReader>();
        services.AddScoped<IJobDeleter, MemoryJobDeleter>();

        return builder;
    }
}
