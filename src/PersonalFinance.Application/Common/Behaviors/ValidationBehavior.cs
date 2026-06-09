using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace PersonalFinance.Application.Common.Behaviors;

/// <summary>
/// Auto-runs FluentValidation before every command/query.
/// Throws ValidationException which the API middleware converts to 422.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators,
    ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (!validators.Any()) return await next();

        logger.LogDebug("Validating {Request}", typeof(TRequest).Name);
        var context = new ValidationContext<TRequest>(request);
        var results = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, ct)));
        var failures = results.SelectMany(r => r.Errors).Where(f => f is not null).ToList();

        if (failures.Count > 0)
        {
            logger.LogWarning("Validation failed for {Request}: {Errors}", typeof(TRequest).Name,
                string.Join(", ", failures.Select(f => f.ErrorMessage)));
            throw new ValidationException(failures);
        }

        return await next();
    }
}
