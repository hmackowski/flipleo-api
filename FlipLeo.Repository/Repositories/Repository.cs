using System.Linq.Expressions;
using FlipLeo.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlipLeo.Repository.Repositories;

public class Repository<TEntity, TContext>(TContext context) : IRepository<TEntity>
    where TEntity : class
    where TContext : DbContext
{
    protected readonly TContext Context = context;
    protected DbSet<TEntity> Set => Context.Set<TEntity>();

    public IQueryable<TEntity> GetAll() => Set;

    public IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> predicate) =>
        Set.Where(predicate);

    public async Task<TEntity?> FindByKeyAsync(params object[] keys) =>
        await Set.FindAsync(keys);

    public Task<TEntity?> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
    {
       return Set.SingleOrDefaultAsync(predicate);
    }

    public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return Set.AnyAsync(predicate);
    }

    public void Add(TEntity entity)
    {
        Set.Add(entity);
    }

    public async Task AddAsync(TEntity entity)
    {
        await Set.AddAsync(entity);
    }

    public void Update(TEntity entity)
    {
        Set.Update(entity);
    }

    public void Delete(TEntity entity)
    {
        Set.Remove(entity);
    }
}