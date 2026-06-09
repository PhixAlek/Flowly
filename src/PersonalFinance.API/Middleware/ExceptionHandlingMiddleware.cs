using FluentValidation;
using System.Net;
using System.Text.Json;

namespace PersonalFinance.API.Middleware;

/// <summary>
/// Global exception handler — translates domain/validation errors to RFC 7807 problem details.
/// OWASP A09: never leak stack traces or internal details in production.
/// </summary>
public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IHostEnvironment env)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (ValidationException ex)
        {
            logger.LogWarning("Validation failed: {Errors}",
                string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));

            ctx.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
            ctx.Response.ContentType = "application/problem+json";

            await ctx.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                type = "https://tools.ietf.org/html/rfc7807",
                title = "Validation Error",
                status = 422,
                errors = ex.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage })
            }));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");

            ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            ctx.Response.ContentType = "application/problem+json";

            // OWASP A09: never expose internals in production
            var detail = env.IsDevelopment() ? ex.ToString() : "An unexpected error occurred.";

            await ctx.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                type = "https://tools.ietf.org/html/rfc7807",
                title = "Internal Server Error",
                status = 500,
                detail
            }));
        }
    }
}
