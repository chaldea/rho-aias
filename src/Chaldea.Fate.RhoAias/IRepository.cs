using System.Linq.Expressions;

namespace Chaldea.Fate.RhoAias;

public interface IRepository<TEntity> where TEntity : class
{
	Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate);
	Task<int> CountAsync();
	Task<TEntity> InsertAsync(TEntity entity);
	Task<TEntity> UpdateAsync(TEntity entity);
	Task UpdateManyAsync(IEnumerable<TEntity> entities);
	Task DeleteAsync(TEntity entity);
	Task DeleteManyAsync(IEnumerable<TEntity> entities);
	Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes);
	Task<List<TEntity>> GetListAsync(params Expression<Func<TEntity, object>>[] includes);
	Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes);
}

internal class MemoryRepository<TEntity> : IRepository<TEntity> where TEntity : class
{
	private readonly List<TEntity> _db = new List<TEntity>();

	public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate)
	{
		var result = _db.Any(predicate.Compile());
		return Task.FromResult(result);
	}

	public Task<int> CountAsync()
	{
		throw new NotImplementedException();
	}

	public Task<TEntity> InsertAsync(TEntity entity)
	{
		_db.Add(entity);
		return Task.FromResult(entity);
	}

	public Task<TEntity> UpdateAsync(TEntity entity)
	{
		return Task.FromResult(entity);
	}

	public Task UpdateManyAsync(IEnumerable<TEntity> entities)
	{
		throw new NotImplementedException();
	}

	public Task DeleteAsync(TEntity entity)
	{
		_db.Remove(entity);
		return Task.CompletedTask;
	}

	public Task DeleteManyAsync(IEnumerable<TEntity> entities)
	{
		throw new NotImplementedException();
	}

	public Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes)
	{
		throw new NotImplementedException();
	}

	public Task<List<TEntity>> GetListAsync(params Expression<Func<TEntity, object>>[] includes)
	{
		return Task.FromResult(_db);
	}

	public Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes)
	{
		return Task.FromResult(_db);
	}
}