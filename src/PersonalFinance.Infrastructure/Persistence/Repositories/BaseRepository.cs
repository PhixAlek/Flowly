using Microsoft.EntityFrameworkCore;
using PersonalFinance.Application.Common.Interfaces;
using PersonalFinance.Domain.Common.Primitives;
using System.Linq.Expressions;

namespace PersonalFinance.Infrastructure.Persistence.Repositories;

public abstract class BaseRepository<TEntity>(AppDbContext context) : IRepository<TEntity>
    where TEntity : Entity
{
    protected readonly AppDbContext Ctx = context;
    protected readonly DbSet<TEntity> Set = context.Set<TEntity>();

    public virtual async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await Set.FirstOrDefaultAsync(e => e.Id == id, ct);

    public virtual async Task<IEnumerable<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) =>
        await Set.Where(predicate).ToListAsync(ct);

    public virtual IQueryable<TEntity> Query() => Set.AsQueryable();

    public virtual async Task AddAsync(TEntity entity, CancellationToken ct = default) =>
        await Set.AddAsync(entity, ct);

    public virtual void Update(TEntity entity) => Set.Update(entity);
    public virtual void Remove(TEntity entity) => Set.Remove(entity);
}
