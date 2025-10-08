using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TheFipster.ActivityAggregator.Domain.Exceptions;

namespace TheFipster.ActivityAggregator.Api.Components;

internal sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Server error",
        };

        if (exception is HttpResponseException)
        {
            var response = (HttpResponseException)exception;
            problemDetails.Title = response.Title;
            problemDetails.Detail = response.Message;
            problemDetails.Status = response.StatusCode;

            _logger.LogError(exception, "Exception occurred: {Message}", exception.Message);
        }
        else
        {
            _logger.LogError(
                exception,
                "Unexpected exception occurred: {Message}",
                exception.Message
            );
        }

        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}
