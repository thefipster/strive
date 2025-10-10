using System.Diagnostics;
using Castle.DynamicProxy;

namespace TheFipster.ActivityAggregator.Api.Interceptors;

public class Tracinginterceptor : IInterceptor
{
    private static readonly ActivitySource ActivitySource = new("ActivityAggregator.Api");

    public void Intercept(IInvocation invocation)
    {
        // Start a new Activity for this method invocation
        using var activity = ActivitySource.StartActivity(
            invocation.Method.Name,
            ActivityKind.Server
        );

        if (activity is not null)
        {
            // Add some useful context
            activity.SetTag("component", invocation.TargetType.FullName);
            activity.SetTag("method.name", invocation.Method.Name);
            activity.SetTag("arguments.count", invocation.Arguments.Length);

            // Optionally: log parameter values (careful with sensitive data)
            for (int i = 0; i < invocation.Arguments.Length; i++)
            {
                var arg = invocation.Arguments[i];
                activity.SetTag($"arg.{i}.type", arg?.GetType().Name ?? "null");
                activity.SetTag($"arg.{i}.value", arg?.ToString());
            }
        }

        try
        {
            invocation.Proceed();

            // If the method returns successfully
            if (activity is not null)
            {
                activity.SetTag("status", "OK");
            }
        }
        catch (Exception ex)
        {
            // Record the exception in the trace
            if (activity is not null)
            {
                activity.SetTag("status", "ERROR");
                activity.SetTag("exception.type", ex.GetType().FullName);
                activity.SetTag("exception.message", ex.Message);
                activity.SetTag("exception.stacktrace", ex.StackTrace);
            }

            throw;
        }
    }
}
