using Castle.DynamicProxy;

namespace TheFipster.ActivityAggregator.Api.Interceptors;

public class VerboseInterceptor(ILogger<VerboseInterceptor> logger) : IInterceptor
{
    public void Intercept(IInvocation invocation)
    {
        logger.LogTrace("Calling {MethodName}", invocation.Method.Name);

        invocation.Proceed();

        logger.LogTrace("Finished {MethodName}", invocation.Method.Name);
    }
}
