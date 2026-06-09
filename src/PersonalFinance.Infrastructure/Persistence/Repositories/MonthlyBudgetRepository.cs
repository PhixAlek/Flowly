using Microsoft.EntityFrameworkCore;
using PersonalFinance.Application.Common.Interfaces;
using PersonalFinance.Application.Common.Models;
using PersonalFinance.Domain.Common.ValueObjects;
using PersonalFinance.Domain.Expense.Entities;

namespace PersonalFinance.Infrastructure.Persistence.Repositories;

public sealed class MonthlyBudgetRepository(AppDbContext ctx)
    : BaseRepository<MonthlyBudget>(ctx), IMonthlyBudgetRepository
{
    public async Task<MonthlyBudget?> GetByUserAndPeriodAsync(Guid userId, DatePeriod period, CancellationToken ct = default) =>
        await Set
            .Include("_entries")
            .FirstOrDefaultAsync(b => b.UserId == userId &&
                EF.Property<int>(b, "PeriodYear") == period.Year &&
                EF.Property<int>(b, "PeriodMonth") == period.Month, ct);

    public async Task<PagedResult<MonthlyBudget>> GetPagedByUserAsync(Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = Set.Where(b => b.UserId == userId).OrderByDescending(b => b.CreatedAtUtc);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).Include("_entries").ToListAsync(ct);
        return new PagedResult<MonthlyBudget> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }
}
