using PersonalFinance.Application.Common.Interfaces;
using PersonalFinance.Application.Common.Models;
using PersonalFinance.Domain.Savings.Entities;
using System.Collections.Concurrent;

namespace PersonalFinance.Infrastructure.Persistence.InMemory;

public sealed class InMemorySavingsGoalRepository(ConcurrentDictionary<Guid, SavingsGoal> store)
    : InMemoryRepository<SavingsGoal>(store), ISavingsGoalRepository
{
    public Task<IEnumerable<SavingsGoal>> GetByUserAsync(
        Guid userId, CancellationToken ct = default) =>
        Task.FromResult(Store.Values.Where(g => g.UserId == userId));

    public Task<PagedResult<SavingsGoal>> GetPagedByUserAsync(
        Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        var items = Store.Values
            .Where(g => g.UserId == userId)
            .OrderByDescending(g => g.CreatedAtUtc)
            .ToList();

        return Task.FromResult(new PagedResult<SavingsGoal>
        {
            Items = items.Skip((page - 1) * pageSize).Take(pageSize),
            TotalCount = items.Count,
            Page = page,
            PageSize = pageSize
        });
    }
}
