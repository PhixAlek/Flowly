using Microsoft.EntityFrameworkCore;
using PersonalFinance.Application.Common.Interfaces;
using PersonalFinance.Application.Common.Models;
using PersonalFinance.Domain.Investment.Entities;

namespace PersonalFinance.Infrastructure.Persistence.Repositories;

public sealed class InvestmentRepository(AppDbContext ctx)
    : BaseRepository<Investment>(ctx), IInvestmentRepository
{
    public async Task<IEnumerable<Investment>> GetByUserAsync(Guid userId, CancellationToken ct = default) =>
        await Set.Where(i => i.UserId == userId).ToListAsync(ct);

    public async Task<PagedResult<Investment>> GetPagedByUserAsync(Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = Set.Where(i => i.UserId == userId).OrderByDescending(i => i.StartDate);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedResult<Investment> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }
}
