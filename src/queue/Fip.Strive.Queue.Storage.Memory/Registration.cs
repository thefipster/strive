using System.Diagnostics.CodeAnalysis;
using Fip.Strive.Queue.Domain;

namespace Fip.Strive.Queue.Storage.Memory;

[ExcludeFromCodeCoverage]
public static class Registration
{
    public static QueueFeatureBuilder WithInMemoryStorage(this QueueFeatureBuilder builder)
    {
        var services = builder.Services;

        return builder;
    }
}
