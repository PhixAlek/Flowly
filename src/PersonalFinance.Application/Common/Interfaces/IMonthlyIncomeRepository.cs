using PersonalFinance.Application.Common.Models;
using PersonalFinance.Domain.Common.ValueObjects;
using PersonalFinance.Domain.Income.Entities;

namespace PersonalFinance.Application.Common.Interfaces;

public interface IMonthlyIncomeRepository : IRepository<MonthlyIncome>
{
    Task<MonthlyIncome?> GetByUserAndPeriodAsync(Guid userId, DatePeriod period, CancellationToken ct = default);
    Task<PagedResult<MonthlyIncome>> GetPagedByUserAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
    Task<bool> ExistsForPeriodAsync(Guid userId, DatePeriod period, CancellationToken ct = default);
}
