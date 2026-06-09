using PersonalFinance.Application.Common.Models;
using PersonalFinance.Domain.Common.ValueObjects;
using PersonalFinance.Domain.Expense.Entities;

namespace PersonalFinance.Application.Common.Interfaces;

public interface IMonthlyBudgetRepository : IRepository<MonthlyBudget>
{
    Task<MonthlyBudget?> GetByUserAndPeriodAsync(Guid userId, DatePeriod period, CancellationToken ct = default);
    Task<PagedResult<MonthlyBudget>> GetPagedByUserAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
}
