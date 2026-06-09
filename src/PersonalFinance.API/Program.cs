using Asp.Versioning;
using PersonalFinance.API.Extensions;
using PersonalFinance.API.Middleware;
using PersonalFinance.Application;
using PersonalFinance.Infrastructure;
using PersonalFinance.Infrastructure.Logging;
using PersonalFinance.Infrastructure.Persistence;
using PersonalFinance.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ── Logging ───────────────────────────────────────────────────────────────
// DIP: Program.cs never references Serilog directly.
builder.Host.UseAppSerilog();

// ── Application (domain use-cases, pipeline behaviours) ──────────────────
builder.Services.AddApplication();

// ── Infrastructure — choose persistence mode ──────────────────────────────
//
//   "UseDatabase": false  →  in-memory stores  (default, no DB needed)
//   "UseDatabase": true   →  SQL Server via EF Core (requires connection string)
//
// To switch to SQL Server:
//   1. Set "UseDatabase": true in appsettings.json (or environment variable)
//   2. Set "ConnectionStrings:DefaultConnection"
//   3. Run: dotnet ef migrations add Init --project src/PersonalFinance.Infrastructure
//           dotnet ef database update       --project src/PersonalFinance.Infrastructure
//
var useDatabase = builder.Configuration.GetValue<bool>("UseDatabase");

if (useDatabase)
    builder.Services.AddInfrastructure(builder.Configuration);
else
    builder.Services.AddInfrastructureInMemory(builder.Configuration);

// ── API ───────────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithJwt();

builder.Services.AddApiVersioning(opt =>
{
    opt.DefaultApiVersion = new ApiVersion(1, 0);
    opt.AssumeDefaultVersionWhenUnspecified = true;
    opt.ReportApiVersions = true;
}).AddApiExplorer(opt =>
{
    opt.GroupNameFormat = "'v'VVV";
    opt.SubstituteApiVersionInUrl = true;
});

// ── Build ─────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── Auto-migrate only when using a real database ──────────────────────────
if (useDatabase && app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PersonalFinance v1"));
}

// ── Middleware pipeline ───────────────────────────────────────────────────
app.UseSecurityHeaders();
app.UseHttpsRedirection();
app.UseSerilogRequestLogging();

var corsSettings = app.Services.GetRequiredService<CorsSettings>();
app.UseCors(corsSettings.PolicyName);

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();

public partial class Program;
