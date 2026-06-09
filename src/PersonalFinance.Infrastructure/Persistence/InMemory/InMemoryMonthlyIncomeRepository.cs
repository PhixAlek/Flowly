using PersonalFinance.Application.Common.Interfaces;
using PersonalFinance.Application.Common.Models;
using PersonalFinance.Domain.Common.ValueObjects;
using PersonalFinance.Domain.Income.Entities;
using System.Collections.Concurrent;

namespace PersonalFinance.Infrastructure.Persistence.InMemory;

public sealed class InMemoryMonthlyIncomeRepository(ConcurrentDictionary<Guid, MonthlyIncome> store)
    : InMemoryRepository<MonthlyIncome>(store), IMonthlyIncomeRepository
{
    public Task<MonthlyIncome?> GetByUserAndPeriodAsync(
        Guid userId, DatePeriod period, CancellationToken ct = default) =>
        Task.FromResult(
            Store.Values.FirstOrDefault(m =>
                m.UserId == userId &&
                m.Period.Year == period.Year &&
                m.Period.Month == period.Month));

    public Task<bool> ExistsForPeriodAsync(
        Guid userId, DatePeriod period, CancellationToken ct = default) =>
        Task.FromResult(
            Store.Values.Any(m =>
                m.UserId == userId &&
                m.Period.Year == period.Year &&
                m.Period.Month == period.Month));

    public Task<PagedResult<MonthlyIncome>> GetPagedByUserAsync(
        Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        var items = Store.Values
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.CreatedAtUtc)
            .ToList();

        return Task.FromResult(new PagedResult<MonthlyIncome>
        {
            Items = items.Skip((page - 1) * pageSize).Take(pageSize),
            TotalCount = items.Count,
            Page = page,
            PageSize = pageSize
        });
    }
}
