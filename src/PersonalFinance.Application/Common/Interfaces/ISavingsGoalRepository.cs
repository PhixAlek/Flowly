using PersonalFinance.Application.Common.Models;
using PersonalFinance.Domain.Savings.Entities;

namespace PersonalFinance.Application.Common.Interfaces;

public interface ISavingsGoalRepository : IRepository<SavingsGoal>
{
    Task<IEnumerable<SavingsGoal>> GetByUserAsync(Guid userId, CancellationToken ct = default);
    Task<PagedResult<SavingsGoal>> GetPagedByUserAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
}
