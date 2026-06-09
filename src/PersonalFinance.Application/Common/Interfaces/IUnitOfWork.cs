namespace PersonalFinance.Application.Common.Interfaces;

/// <summary>Port: persistence unit-of-work. Infrastructure implements this.</summary>
public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
