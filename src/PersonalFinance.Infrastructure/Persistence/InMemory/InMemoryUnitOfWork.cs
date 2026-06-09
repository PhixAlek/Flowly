using PersonalFinance.Application.Common.Interfaces;

namespace PersonalFinance.Infrastructure.Persistence.InMemory;

/// <summary>
/// No-op unit of work for in-memory use.
/// There is no transaction to commit — the in-memory stores are already
/// updated by the repository methods directly.
/// </summary>
public sealed class InMemoryUnitOfWork : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(0);

    public void Dispose() { }
}
