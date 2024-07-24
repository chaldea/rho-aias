using System.Linq.Expressions;

namespace Chaldea.Fate.RhoAias;

internal class InMemoryRepository<TEntity> : IRepository<TEntity> where TEntity : class
{
    private readonly InMemoryDbContext _dbContext;

    public InMemoryRepository(InMemoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate)
    {
        var result = _dbContext.Set<TEntity>().Any(predicate);
        return Task.FromResult(result);
    }

    public Task<int> CountAsync()
    {
        var count = _dbContext.Set<TEntity>().Count();
        return Task.FromResult(count);
    }

    public Task<TEntity> InsertAsync(TEntity entity)
    {
        _dbContext.Set<TEntity>().Add(entity);
        return Task.FromResult(entity);
    }

    public Task InsertManyAsync(IEnumerable<TEntity> entities)
    {
        _dbContext.Set<TEntity>().AddRange(entities);
        return Task.CompletedTask;
    }

    public Task<TEntity> UpdateAsync(TEntity entity)
    {
        var result = _dbContext.Set<TEntity>().Update(entity);
        return Task.FromResult(result);
    }

    public Task UpdateManyAsync(IEnumerable<TEntity> entities)
    {
        _dbContext.Set<TEntity>().UpdateRange(entities);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(TEntity entity)
    {
        _dbContext.Set<TEntity>().Remove(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Expression<Func<TEntity, bool>> predicate)
    {
        var entities = _dbContext.Set<TEntity>().ToList();
        _dbContext.Set<TEntity>().RemoveRange(entities);
        return Task.CompletedTask;
    }

    public Task DeleteManyAsync(IEnumerable<TEntity> entities)
    {
        _dbContext.Set<TEntity>().RemoveRange(entities);
        return Task.CompletedTask;
    }

    public Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes)
    {
        var result = _dbContext.Set<TEntity>().FirstOrDefault(predicate, includes);
        return Task.FromResult(result);
    }

    public Task<List<TEntity>> GetListAsync(params Expression<Func<TEntity, object>>[] includes)
    {
        var result = _dbContext.Set<TEntity>().ToList(includes);
        return Task.FromResult(result);
    }

    public Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes)
    {
        var items = _dbContext.Set<TEntity>().ToList(predicate, includes);
        return Task.FromResult(items);
    }
}
