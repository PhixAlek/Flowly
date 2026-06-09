using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PersonalFinance.Application.Common.Interfaces;
using PersonalFinance.Application.Income.Ports;
using PersonalFinance.Domain.Expense.Entities;
using PersonalFinance.Domain.Income.Entities;
using PersonalFinance.Domain.Investment.Entities;
using PersonalFinance.Domain.Savings.Entities;
using PersonalFinance.Infrastructure.Adapters.Currency;
using PersonalFinance.Infrastructure.Adapters.Identity;
using PersonalFinance.Infrastructure.Persistence;
using PersonalFinance.Infrastructure.Persistence.InMemory;
using PersonalFinance.Infrastructure.Persistence.Repositories;
using PersonalFinance.Infrastructure.Security;
using System.Collections.Concurrent;
using System.Text;

namespace PersonalFinance.Infrastructure;

public static class DependencyInjection
{
    // ── Path A: In-Memory (default — no DB required) ─────────────────────
    /// <summary>
    /// Registers all infrastructure services using in-memory stores.
    /// The application starts and fully exercises domain logic with zero
    /// external dependencies. Use this during development and until a real
    /// database is ready.
    /// Data does NOT persist between application restarts.
    /// </summary>
    public static IServiceCollection AddInfrastructureInMemory(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Each store is a singleton dictionary — one shared store per
        // aggregate type for the lifetime of the process.
        services.AddSingleton(new ConcurrentDictionary<Guid, MonthlyIncome>());
        services.AddSingleton(new ConcurrentDictionary<Guid, MonthlyBudget>());
        services.AddSingleton(new ConcurrentDictionary<Guid, SavingsGoal>());
        services.AddSingleton(new ConcurrentDictionary<Guid, Investment>());

        services.AddScoped<IUnitOfWork, InMemoryUnitOfWork>();
        services.AddScoped<IMonthlyIncomeRepository, InMemoryMonthlyIncomeRepository>();
        services.AddScoped<IMonthlyBudgetRepository, InMemoryMonthlyBudgetRepository>();
        services.AddScoped<ISavingsGoalRepository, InMemorySavingsGoalRepository>();
        services.AddScoped<IInvestmentRepository, InMemoryInvestmentRepository>();

        services
            .AddSharedAdapters(configuration)
            .AddSecurity(configuration);

        return services;
    }

    // ── Path B: SQL Server + EF Core (production) ────────────────────────
    /// <summary>
    /// Registers all infrastructure services backed by SQL Server via EF Core.
    /// Requires "ConnectionStrings:DefaultConnection" in configuration.
    /// Run 'dotnet ef migrations add InitialCreate' and 'dotnet ef database update'
    /// before switching to this path.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(opt =>
            opt.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql =>
                {
                    sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                    sql.EnableRetryOnFailure(3);
                }));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IMonthlyIncomeRepository, MonthlyIncomeRepository>();
        services.AddScoped<IMonthlyBudgetRepository, MonthlyBudgetRepository>();
        services.AddScoped<ISavingsGoalRepository, SavingsGoalRepository>();
        services.AddScoped<IInvestmentRepository, InvestmentRepository>();

        services
            .AddSharedAdapters(configuration)
            .AddSecurity(configuration);

        return services;
    }

    // ── Shared: adapters that are the same regardless of the DB choice ────
    private static IServiceCollection AddSharedAdapters(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Currency converter (ExchangeRate-API)
        services.Configure<ExchangeRateSettings>(
            configuration.GetSection(ExchangeRateSettings.SectionName));
        services.AddMemoryCache();
        services.AddHttpClient<ICurrencyConverter, ExchangeRateApiAdapter>(client =>
            client.Timeout = TimeSpan.FromSeconds(10));

        // JWT
        var jwtSettings = configuration
            .GetSection(JwtSettings.SectionName)
            .Get<JwtSettings>()
            ?? throw new InvalidOperationException("JwtSettings must be configured.");

        services.Configure<JwtSettings>(
            configuration.GetSection(JwtSettings.SectionName));
        services.AddSingleton<JwtTokenService>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opt => opt.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer           = true,
                ValidateAudience         = true,
                ValidateLifetime         = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer              = jwtSettings.Issuer,
                ValidAudience            = jwtSettings.Audience,
                IssuerSigningKey         = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                ClockSkew = TimeSpan.Zero
            });

        services.AddAuthorization();

        return services;
    }
}
