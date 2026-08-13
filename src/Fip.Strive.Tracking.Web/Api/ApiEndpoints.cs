using Fip.Strive.Tracking.Application.Features.Access.Services.Contracts;
using Fip.Strive.Tracking.Application.Features.Export.Services.Contracts;

namespace Fip.Strive.Tracking.Web.Api;

public static class ApiEndpoints
{
    /// <summary>
    /// The read-only side the homelab pulls with. Not mapped at all when no API key is configured,
    /// so an instance that does not want an API does not merely reject calls — it has no API.
    /// </summary>
    public static void MapTrackingApi(this WebApplication app)
    {
        var guard = app.Services.GetRequiredService<IAccessGuard>();

        if (!guard.ApiEnabled)
        {
            app.Logger.LogInformation(
                "No {Section} key configured — the pull API is off",
                "Access:ApiKey"
            );
            return;
        }

        var api = app.MapGroup("/api/v1").AddEndpointFilter<ApiKeyFilter>();

        api.MapGet(
            "/trackers",
            async (IExportReader export, CancellationToken cancellationToken) =>
                Results.Ok(await export.GetTrackersAsync(cancellationToken))
        );

        api.MapGet(
            "/events",
            async (
                IExportReader export,
                CancellationToken cancellationToken,
                DateTimeOffset? since = null,
                Guid? trackerId = null,
                int skip = 0,
                int take = IExportReader.DefaultPageSize
            ) =>
                Results.Ok(
                    await export.GetEventsAsync(since, trackerId, skip, take, cancellationToken)
                )
        );

        app.Logger.LogInformation(
            "Pull API is on at /api/v1, keyed by {Header}",
            ApiKeyFilter.HeaderName
        );
    }
}
