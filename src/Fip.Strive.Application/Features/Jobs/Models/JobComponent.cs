namespace Fip.Strive.Application.Features.Jobs.Models;

/// <summary>
/// A handler's identity, without the handler. Held by the registry so a singleton never retains a
/// scoped handler — and so step 3's invalidation sweep has something to compare stored versions
/// against without resolving anything.
/// </summary>
public sealed record JobComponent(string Kind, string ComponentId, int Version);
