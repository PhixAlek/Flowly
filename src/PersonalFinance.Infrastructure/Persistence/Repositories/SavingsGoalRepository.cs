using Microsoft.EntityFrameworkCore;
using PersonalFinance.Application.Common.Interfaces;
using PersonalFinance.Application.Common.Models;
using PersonalFinance.Domain.Savings.Entities;

namespace PersonalFinance.Infrastructure.Persistence.Repositories;

public sealed class SavingsGoalRepository(AppDbContext ctx)
    : BaseRepository<SavingsGoal>(ctx), ISavingsGoalRepository
{
    public async Task<IEnumerable<SavingsGoal>> GetByUserAsync(Guid userId, CancellationToken ct = default) =>
        await Set.Where(g => g.UserId == userId).ToListAsync(ct);

    public async Task<PagedResult<SavingsGoal>> GetPagedByUserAsync(Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = Set.Where(g => g.UserId == userId).OrderByDescending(g => g.CreatedAtUtc);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedResult<SavingsGoal> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }
}
