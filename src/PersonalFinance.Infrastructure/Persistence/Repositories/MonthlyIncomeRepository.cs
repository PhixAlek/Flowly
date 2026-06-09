using Microsoft.EntityFrameworkCore;
using PersonalFinance.Application.Common.Interfaces;
using PersonalFinance.Application.Common.Models;
using PersonalFinance.Domain.Common.ValueObjects;
using PersonalFinance.Domain.Income.Entities;

namespace PersonalFinance.Infrastructure.Persistence.Repositories;

public sealed class MonthlyIncomeRepository(AppDbContext ctx)
    : BaseRepository<MonthlyIncome>(ctx), IMonthlyIncomeRepository
{
    public async Task<MonthlyIncome?> GetByUserAndPeriodAsync(Guid userId, DatePeriod period, CancellationToken ct = default) =>
        await Set
            .Include("_entries")
            .FirstOrDefaultAsync(m => m.UserId == userId &&
                EF.Property<int>(m, "PeriodYear") == period.Year &&
                EF.Property<int>(m, "PeriodMonth") == period.Month, ct);

    public async Task<bool> ExistsForPeriodAsync(Guid userId, DatePeriod period, CancellationToken ct = default) =>
        await Set.AnyAsync(m => m.UserId == userId &&
            EF.Property<int>(m, "PeriodYear") == period.Year &&
            EF.Property<int>(m, "PeriodMonth") == period.Month, ct);

    public async Task<PagedResult<MonthlyIncome>> GetPagedByUserAsync(Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = Set.Where(m => m.UserId == userId).OrderByDescending(m => m.CreatedAtUtc);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).Include("_entries").ToListAsync(ct);
        return new PagedResult<MonthlyIncome> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }
}
