using PersonalFinance.Application.Common.Interfaces;
using PersonalFinance.Application.Common.Models;
using PersonalFinance.Domain.Common.ValueObjects;
using PersonalFinance.Domain.Expense.Entities;
using System.Collections.Concurrent;

namespace PersonalFinance.Infrastructure.Persistence.InMemory;

public sealed class InMemoryMonthlyBudgetRepository(ConcurrentDictionary<Guid, MonthlyBudget> store)
    : InMemoryRepository<MonthlyBudget>(store), IMonthlyBudgetRepository
{
    public Task<MonthlyBudget?> GetByUserAndPeriodAsync(
        Guid userId, DatePeriod period, CancellationToken ct = default) =>
        Task.FromResult(
            Store.Values.FirstOrDefault(b =>
                b.UserId == userId &&
                b.Period.Year == period.Year &&
                b.Period.Month == period.Month));

    public Task<PagedResult<MonthlyBudget>> GetPagedByUserAsync(
        Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        var items = Store.Values
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAtUtc)
            .ToList();

        return Task.FromResult(new PagedResult<MonthlyBudget>
        {
            Items = items.Skip((page - 1) * pageSize).Take(pageSize),
            TotalCount = items.Count,
            Page = page,
            PageSize = pageSize
        });
    }
}
