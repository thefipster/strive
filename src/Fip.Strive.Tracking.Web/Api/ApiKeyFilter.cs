using Fip.Strive.Tracking.Application.Features.Access.Services.Contracts;

namespace Fip.Strive.Tracking.Web.Api;

/// <summary>
/// Guards the pull API with the shared key. A filter rather than an authentication scheme: nothing
/// downstream needs an identity, only a yes or a no, and this way the API is visibly independent of
/// the cookie the browser uses.
/// </summary>
public sealed class ApiKeyFilter(IAccessGuard guard, ILogger<ApiKeyFilter> logger) : IEndpointFilter
{
    public const string HeaderName = "X-Api-Key";

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next
    )
    {
        var supplied = context.HttpContext.Request.Headers[HeaderName].ToString();

        if (guard.IsApiKey(supplied))
            return await next(context);

        logger.LogWarning(
            "Rejected an API call to {Path} from {Address}",
            context.HttpContext.Request.Path,
            context.HttpContext.Connection.RemoteIpAddress
        );

        return Results.Json(
            new { error = $"Missing or invalid {HeaderName} header." },
            statusCode: StatusCodes.Status401Unauthorized
        );
    }
}
