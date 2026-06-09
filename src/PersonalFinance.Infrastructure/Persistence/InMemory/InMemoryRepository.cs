using PersonalFinance.Application.Common.Interfaces;
using PersonalFinance.Domain.Common.Primitives;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace PersonalFinance.Infrastructure.Persistence.InMemory;

/// <summary>
/// Thread-safe, in-memory generic repository.
/// Uses a ConcurrentDictionary keyed by entity Id.
/// This is the stand-in for EF Core until a real database is wired.
/// </summary>
public abstract class InMemoryRepository<TEntity>(ConcurrentDictionary<Guid, TEntity> store)
    : IRepository<TEntity>
    where TEntity : Entity
{
    protected readonly ConcurrentDictionary<Guid, TEntity> Store = store;

    public Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(Store.TryGetValue(id, out var entity) ? entity : null);

    public Task<IEnumerable<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) =>
        Task.FromResult(Store.Values.Where(predicate.Compile()));

    public IQueryable<TEntity> Query() =>
        Store.Values.AsQueryable();

    public Task AddAsync(TEntity entity, CancellationToken ct = default)
    {
        Store[entity.Id] = entity;
        return Task.CompletedTask;
    }

    public void Update(TEntity entity) =>
        Store[entity.Id] = entity;

    public void Remove(TEntity entity) =>
        Store.TryRemove(entity.Id, out _);
}
