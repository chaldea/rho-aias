using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Chaldea.Fate.RhoAias.Repository.Sqlite
{
	internal class SqliteRepository<TEntity> : IRepository<TEntity> where TEntity : class
	{
		private readonly IDbContextFactory<RhoAiasDbContext> _contextFactory;

		public SqliteRepository(IDbContextFactory<RhoAiasDbContext> contextFactory)
		{
			_contextFactory = contextFactory;
		}

		public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
		{
			await using var context = _contextFactory.CreateDbContext();
			return await context.Set<TEntity>().AnyAsync(predicate, cancellationToken);
		}

		public async Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
		{
			await using var context = _contextFactory.CreateDbContext();
			var r = await context.Set<TEntity>().AddAsync(entity);
			await context.SaveChangesAsync();
			return r.Entity;
		}

		public async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken))
		{
			await using var context = _contextFactory.CreateDbContext();
			var r = context.Set<TEntity>().Update(entity);
			await context.SaveChangesAsync();
			return r.Entity;
		}

		public async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
		{
			await using var context = _contextFactory.CreateDbContext();
			context.Set<TEntity>().Remove(entity);
			await context.SaveChangesAsync();
		}

		public async Task DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
		{
			await using var context = _contextFactory.CreateDbContext();
			var list = await context.Set<TEntity>().Where(predicate).ToArrayAsync();
			context.Set<TEntity>().RemoveRange(list);
			await context.SaveChangesAsync();
		}

		public async Task DeleteManyAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default(CancellationToken))
		{
			await using var context = _contextFactory.CreateDbContext();
			context.Set<TEntity>().RemoveRange(entities);
			await context.SaveChangesAsync();
		}

		public async Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes)
		{
			await using var context = _contextFactory.CreateDbContext();
			var query = context.Set<TEntity>().AsQueryable();
			if (includes is { Length: > 0 })
			{
				foreach (var propertySelector in includes)
				{
					query = query.Include(propertySelector);
				}
			}
			return await query.Where(predicate).FirstOrDefaultAsync();
		}

		public async Task<List<TEntity>> GetListAsync(params Expression<Func<TEntity, object>>[] includes)
		{
			await using var context = _contextFactory.CreateDbContext();
			var query = context.Set<TEntity>().AsQueryable();
			if (includes is { Length: > 0 })
			{
				foreach (var propertySelector in includes)
				{
					query = query.Include(propertySelector);
				}
			}
			return await query.ToListAsync();
		}

		public async Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes)
		{
			await using var context = _contextFactory.CreateDbContext();
			var query = context.Set<TEntity>().AsQueryable();
			if (includes is { Length: > 0 })
			{
				foreach (var propertySelector in includes)
				{
					query = query.Include(propertySelector);
				}
			}
			return await query.Where(predicate).ToListAsync();
		}
	}
}
