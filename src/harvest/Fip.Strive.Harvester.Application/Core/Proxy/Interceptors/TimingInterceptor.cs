using System.Diagnostics;
using Castle.DynamicProxy;
using Microsoft.Extensions.Logging;

namespace Fip.Strive.Harvester.Application.Core.Interceptors;

public class TimingInterceptor(ILogger<TimingInterceptor> logger) : IInterceptor
{
    public void Intercept(IInvocation invocation)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        invocation.Proceed();

        stopwatch.Stop();
        logger.LogInformation(
            "Finished {TypeName}.{MethodName} in {RuntimeMs:N0} ms",
            invocation.Method.DeclaringType?.Name ?? "Unknown",
            invocation.Method.Name,
            stopwatch.ElapsedMilliseconds
        );
    }
}
