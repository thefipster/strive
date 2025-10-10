using System.Diagnostics;
using Castle.DynamicProxy;

namespace TheFipster.ActivityAggregator.Api.Interceptors;

public class TracingInterceptor : IInterceptor
{
    private static readonly ActivitySource ActivitySource = new(nameof(Api));

    public void Intercept(IInvocation invocation)
    {
        using var activity = ActivitySource.StartActivity(invocation.Method.Name);

        if (activity is not null)
        {
            activity.SetTag("component", invocation.TargetType?.FullName);
            activity.SetTag("method.name", invocation.Method.Name);
            activity.SetTag("arguments.count", invocation.Arguments.Length);
        }

        try
        {
            invocation.Proceed();

            if (activity is not null)
                activity.SetTag("status", "OK");
        }
        catch (Exception ex)
        {
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
