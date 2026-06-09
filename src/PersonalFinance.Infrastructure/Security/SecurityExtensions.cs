using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PersonalFinance.Infrastructure.Security;

/// <summary>
/// Centralises CORS + OWASP Top 10 mitigations:
///   A01 – Broken Access Control   → JWT + [Authorize] on all endpoints
///   A02 – Cryptographic Failures  → HTTPS enforced, JWT HS256 with strong key
///   A03 – Injection               → EF Core parameterised queries, no raw SQL
///   A05 – Security Misconfiguration → security headers middleware
///   A06 – Vulnerable Components   → packages pinned, TreatWarningsAsErrors
///   A07 – Auth failures           → JWT with short expiry, no sensitive data in token
///   A09 – Logging failures        → Serilog captures all requests and errors
/// </summary>
public static class SecurityExtensions
{
    public static IServiceCollection AddSecurity(this IServiceCollection services, IConfiguration configuration)
    {
        var corsSettings = configuration.GetSection(CorsSettings.SectionName).Get<CorsSettings>()
            ?? new CorsSettings { AllowedOrigins = [] };

        services.AddCors(opt => opt.AddPolicy(corsSettings.PolicyName, policy =>
        {
            if (corsSettings.AllowedOrigins.Length == 0)
                // Dev: allow any localhost origin
                policy.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            else
                policy.WithOrigins(corsSettings.AllowedOrigins)
                      .AllowAnyHeader()
                      .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH")
                      .AllowCredentials();
        }));

        services.AddSingleton(corsSettings);
        return services;
    }

    /// <summary>
    /// OWASP A05: adds security response headers.
    /// Call this early in the middleware pipeline.
    /// </summary>
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        app.Use(async (ctx, next) =>
        {
            // Prevent MIME-type sniffing (A05)
            ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
            // Prevent clickjacking
            ctx.Response.Headers["X-Frame-Options"] = "DENY";
            // XSS protection (legacy browsers)
            ctx.Response.Headers["X-XSS-Protection"] = "1; mode=block";
            // Disable browser caching for API responses
            ctx.Response.Headers["Cache-Control"] = "no-store";
            ctx.Response.Headers["Pragma"] = "no-cache";
            // HTTPS only (ensure HSTS is also configured)
            ctx.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
            // Referrer policy
            ctx.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            // Remove server banner
            ctx.Response.Headers.Remove("Server");

            await next();
        });

        return app;
    }
}
