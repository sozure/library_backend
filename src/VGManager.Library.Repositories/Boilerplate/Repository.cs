using Microsoft.EntityFrameworkCore;
using VGManager.Library.Repositories.Interfaces.Boilerplate;

namespace VGManager.Library.Repositories.Boilerplate;

public abstract class Repository<TEntity> : IRepository<TEntity> where TEntity : class
{
    //
    // Summary:
    //     The DbContext
    protected DbContext _dbContext;

    //
    // Summary:
    //     The Entity
    protected DbSet<TEntity> _dbSet;

    //
    // Summary:
    //     Initialize a new instance of Repository.Core.Repository`1
    //
    // Parameters:
    //   dbContext:
    //     The current DbContext
    protected Repository(DbContext dbContext)
    {
        _dbContext = dbContext;
        _dbSet = _dbContext.Set<TEntity>();
    }

    public virtual Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        return _dbSet.AddRangeAsync(entities, cancellationToken);
    }

    public virtual ValueTask<TEntity?> FindAsync(object[] keys, CancellationToken cancellationToken = default)
    {
        return _dbSet.FindAsync(keys, cancellationToken);
    }

    public virtual void Remove(TEntity entity)
    {
        _dbSet.Remove(entity);
    }

    public virtual void RemoveRange(IEnumerable<TEntity> entities)
    {
        _dbSet.RemoveRange(entities);
    }

    public virtual void UpdateRange(IEnumerable<TEntity> entities)
    {
        _dbSet.UpdateRange(entities);
    }
}
