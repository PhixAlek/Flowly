using MediatR;
using Microsoft.EntityFrameworkCore;
using PersonalFinance.Application.Common.Interfaces;
using PersonalFinance.Domain.Common.Primitives;
using PersonalFinance.Domain.Expense.Entities;
using PersonalFinance.Domain.Income.Entities;
using PersonalFinance.Domain.Investment.Entities;
using PersonalFinance.Domain.Savings.Entities;

namespace PersonalFinance.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options, IMediator mediator)
    : DbContext(options), IUnitOfWork
{
    // ── Aggregates ────────────────────────────────────────────────────────
    public DbSet<MonthlyIncome> MonthlyIncomes => Set<MonthlyIncome>();
    public DbSet<MonthlyBudget> MonthlyBudgets => Set<MonthlyBudget>();
    public DbSet<SavingsGoal> SavingsGoals => Set<SavingsGoal>();
    public DbSet<Investment> Investments => Set<Investment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(builder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        await DispatchDomainEventsAsync(ct);
        return await base.SaveChangesAsync(ct);
    }

    private async Task DispatchDomainEventsAsync(CancellationToken ct)
    {
        var aggregates = ChangeTracker.Entries<Entity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var events = aggregates.SelectMany(a => a.DomainEvents).ToList();
        aggregates.ForEach(a => a.ClearDomainEvents());

        // Domain events dispatched in-process via MediatR — no external queue needed for now.
        foreach (var @event in events)
            await mediator.Publish(@event, ct);
    }
}
