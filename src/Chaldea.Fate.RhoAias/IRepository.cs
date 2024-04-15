using System.Linq.Expressions;

namespace Chaldea.Fate.RhoAias;

public interface IRepository<TEntity> where TEntity : class
{
	Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

	Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default);

	Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

	Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

	Task DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
	Task DeleteManyAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
	Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes);

	Task<List<TEntity>> GetListAsync(params Expression<Func<TEntity, object>>[] includes);
	Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes);
}

internal class MemoryRepository<TEntity> : IRepository<TEntity> where TEntity : class
{
	private readonly List<TEntity> _db = new List<TEntity>();

	public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
	{
		var result = _db.Any(predicate.Compile());
		return Task.FromResult(result);
	}

	public Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
	{
		_db.Add(entity);
		return Task.FromResult(entity);
	}

	public Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken))
	{
		return Task.FromResult(entity);
	}

	public Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
	{
		_db.Remove(entity);
		return Task.CompletedTask;
	}

	public Task DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
	{
		var item = _db.FirstOrDefault(predicate.Compile());
		if (item != null)
			_db.Remove(item);
		return Task.CompletedTask;
	}

	public Task DeleteManyAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes)
	{
		throw new NotImplementedException();
	}

	public Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate, bool includeDetails = true, CancellationToken cancellationToken = default)
	{
		var item = _db.FirstOrDefault(predicate.Compile());
		return Task.FromResult(item);
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