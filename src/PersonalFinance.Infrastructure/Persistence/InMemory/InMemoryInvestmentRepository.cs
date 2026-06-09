using PersonalFinance.Application.Common.Interfaces;
using PersonalFinance.Application.Common.Models;
using PersonalFinance.Domain.Investment.Entities;
using System.Collections.Concurrent;

namespace PersonalFinance.Infrastructure.Persistence.InMemory;

public sealed class InMemoryInvestmentRepository(ConcurrentDictionary<Guid, Investment> store)
    : InMemoryRepository<Investment>(store), IInvestmentRepository
{
    public Task<IEnumerable<Investment>> GetByUserAsync(
        Guid userId, CancellationToken ct = default) =>
        Task.FromResult(Store.Values.Where(i => i.UserId == userId));

    public Task<PagedResult<Investment>> GetPagedByUserAsync(
        Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        var items = Store.Values
            .Where(i => i.UserId == userId)
            .OrderByDescending(i => i.StartDate)
            .ToList();

        return Task.FromResult(new PagedResult<Investment>
        {
            Items = items.Skip((page - 1) * pageSize).Take(pageSize),
            TotalCount = items.Count,
            Page = page,
            PageSize = pageSize
        });
    }
}
