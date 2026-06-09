using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace PersonalFinance.Application.Common.Behaviors;

/// <summary>Logs every request with elapsed time. Warns if over 500ms.</summary>
public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private const int SlowRequestThresholdMs = 500;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var name = typeof(TRequest).Name;
        logger.LogInformation("→ {Request}", name);
        var sw = Stopwatch.StartNew();
        try
        {
            var response = await next();
            sw.Stop();
            if (sw.ElapsedMilliseconds > SlowRequestThresholdMs)
                logger.LogWarning("SLOW {Request} took {Ms}ms", name, sw.ElapsedMilliseconds);
            else
                logger.LogInformation("✓ {Request} in {Ms}ms", name, sw.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogError(ex, "✗ {Request} failed after {Ms}ms", name, sw.ElapsedMilliseconds);
            throw;
        }
    }
}
