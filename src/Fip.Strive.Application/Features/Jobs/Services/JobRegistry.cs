using Fip.Strive.Application.Features.Jobs.Models;
using Fip.Strive.Application.Features.Jobs.Services.Contracts;

namespace Fip.Strive.Application.Features.Jobs.Services;

public sealed class JobRegistry : IJobRegistry
{
    private readonly Dictionary<string, JobComponent> _byKind;

    public JobRegistry(IEnumerable<IJobHandler> handlers)
    {
        _byKind = new Dictionary<string, JobComponent>(StringComparer.Ordinal);

        foreach (var handler in handlers)
        {
            var component = new JobComponent(handler.Kind, handler.ComponentId, handler.Version);

            // Thrown at construction, which means at startup, rather than the first time a job of
            // the ambiguous kind happens to be claimed.
            if (!_byKind.TryAdd(handler.Kind, component))
                throw new InvalidOperationException(
                    $"Two job handlers claim the kind '{handler.Kind}': "
                        + $"{_byKind[handler.Kind].ComponentId} and {handler.ComponentId}."
                );
        }
    }

    public IReadOnlyCollection<JobComponent> All => _byKind.Values;

    public JobComponent Resolve(string kind) =>
        _byKind.TryGetValue(kind, out var component)
            ? component
            : throw new InvalidOperationException(
                $"No job handler is registered for the kind '{kind}'. A row of this kind exists, "
                    + "so either its handler was removed or its registration is missing."
            );
}
