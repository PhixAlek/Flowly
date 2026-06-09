using PersonalFinance.Application.Common.Models;

using InvestmentEntity = PersonalFinance.Domain.Investment.Entities.Investment;

namespace PersonalFinance.Application.Common.Interfaces;


public interface IInvestmentRepository : IRepository<InvestmentEntity>
{
    Task<IEnumerable<InvestmentEntity>> GetByUserAsync(Guid userId, CancellationToken ct = default);
    Task<PagedResult<InvestmentEntity>> GetPagedByUserAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
}