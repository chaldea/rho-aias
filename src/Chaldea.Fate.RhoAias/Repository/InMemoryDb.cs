﻿using System.Linq.Expressions;

namespace Chaldea.Fate.RhoAias;

internal class InMemoryDbContext : DbContext
{
    public DbSet<Client> Clients { get; set; }
    public DbSet<Proxy> Proxies { get; set; }
    public DbSet<Cert> Certs { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<DnsProvider> DnsProviders { get; set; }
}

internal class DbContext
{
    private readonly Dictionary<Type, IDbSet> _tables = new();

    public DbContext()
    {
        InitTables();
    }

    private void InitTables()
    {
        foreach (var propertyInfo in GetType().GetProperties())
        {
            var propType = propertyInfo.PropertyType;
            if (!propType.IsGenericType)
                continue;

            var typeDef = propType.GetGenericTypeDefinition();
            if (typeDef == typeof(DbSet<>))
            {
                var args = propType.GetGenericArguments();
                var dbType = args[0];
                var table = Activator.CreateInstance(typeof(DbSet<>).MakeGenericType(dbType), this);
                _tables.TryAdd(dbType, table as IDbSet);
                propertyInfo.SetValue(this, table);
            }
        }
    }

    public Dictionary<Type, IDbSet> GetAllDbSets()
    {
        return _tables;
    }

    public DbSet<TEntity> Set<TEntity>()
    {
        if (_tables.TryGetValue(typeof(TEntity), out var db))
        {
            return db as DbSet<TEntity>;
        }

        throw new Exception("Invaild db type.");
    }
}

internal interface IDbSet
{
    object? Include(string field, object value, bool isList);
}

internal class DbSet<TEntity> : IDbSet
{
    private readonly List<TEntity> _set;
    private readonly DbContext _context;

    public DbSet(DbContext context)
    {
        _set = [];
        _context = context;
    }

    public bool Any(Expression<Func<TEntity, bool>> predicate)
    {
        return _set.Any(predicate.Compile());
    }

    public int Count()
    {
        return _set.Count();
    }

    public TEntity Add(TEntity entity)
    {
        _set.Add(entity);
        return entity;
    }

    public void AddRange(IEnumerable<TEntity> entities)
    {
        _set.AddRange(entities);
    }

    public TEntity Update(TEntity entity)
    {
        return entity;
    }

    public void UpdateRange(IEnumerable<TEntity> entities)
    {
        return;
    }

    public void Remove(TEntity entity)
    {
        _set.Remove(entity);
    }

    public void RemoveRange(IEnumerable<TEntity> entities)
    {
        _set.RemoveAll(x => entities.Contains(x));
    }

    public TEntity? FirstOrDefault(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes)
    {
        var entity = _set.Where(predicate.Compile()).FirstOrDefault();
        if (entity == null) return default;
        Include(entity, includes);
        return entity;
    }

    public List<TEntity> ToList()
    {
        return _set;
    }

    public List<TEntity> ToList(params Expression<Func<TEntity, object>>[] includes)
    {
        foreach (var item in _set)
        {
            Include(item, includes);
        }

        return _set;
    }

    public List<TEntity> ToList(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes)
    {
        var items = _set.Where(predicate.Compile()).ToList();
        foreach (var item in items)
        {
            Include(item, includes);
        }

        return _set;
    }

    public object? Include(string field, object value, bool isList)
    {
        var type = typeof(TEntity);
        var valExp = Expression.Constant(value);
        var paramExp = Expression.Parameter(type, "x");
        var propExp = Expression.Property(paramExp, field);
        var equalExp = Expression.Equal(propExp, valExp);
        var predicate = Expression.Lambda<Func<TEntity, bool>>(equalExp, paramExp);
        if (isList) return _set.Where(predicate.Compile()).ToList();
        return _set.FirstOrDefault(predicate.Compile());
    }

    private void Include(TEntity entity, params Expression<Func<TEntity, object>>[] includes)
    {
        if (includes.Length <= 0) return;
        var t = typeof(TEntity);
        var incDbSets = _context.GetAllDbSets();
        foreach (var exp in includes)
        {
            var dbType = exp.Body.Type;
            if (dbType.IsGenericType)
            {
                var args = dbType.GetGenericArguments();
                var realType = args[0];
                if (incDbSets.TryGetValue(realType, out var db) && exp.Body is MemberExpression body)
                {
                    var value = t.GetProperty("Id")?.GetValue(entity);
                    var incEntities = db.Include($"{t.Name}Id", value, true);
                    t.GetProperty(body.Member.Name)?.SetValue(entity, incEntities);
                }
            }
            else
            {
                if (incDbSets.TryGetValue(dbType, out var db) && exp.Body is MemberExpression body)
                {
                    var memName = body.Member.Name;
                    var value = t.GetProperty($"{memName}Id")?.GetValue(entity);
                    var incEntity = db.Include("Id", value, false);
                    t.GetProperty(memName)?.SetValue(entity, incEntity);
                }
            }
        }
    }
}
