using System.Linq.Expressions;

namespace Chaldea.Fate.RhoAias;

public interface IRepository<TEntity> where TEntity : class
{
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate);
    Task<int> CountAsync();
    Task<TEntity> InsertAsync(TEntity entity);
    Task InsertManyAsync(IEnumerable<TEntity> entities);
    Task<TEntity> UpdateAsync(TEntity entity);
    Task UpdateManyAsync(IEnumerable<TEntity> entities);
    Task DeleteAsync(TEntity entity);
    Task DeleteAsync(Expression<Func<TEntity, bool>> predicate);
    Task DeleteManyAsync(IEnumerable<TEntity> entities);
    Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes);
    Task<List<TEntity>> GetListAsync(params Expression<Func<TEntity, object>>[] includes);
    Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes);
}
