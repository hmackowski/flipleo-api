using System.Linq.Expressions;

namespace FlipLeo.Repository.Interfaces;

public interface IRepository<TEntity> where TEntity : class
{
    IQueryable<TEntity> GetAll();
    IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);
    Task<TEntity?> FindByKeyAsync(params object[] keys);
    Task<TEntity?> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate);
    void Add(TEntity entity);
    Task AddAsync(TEntity entity);
    void Update(TEntity entity);
    void Delete(TEntity entity);
}