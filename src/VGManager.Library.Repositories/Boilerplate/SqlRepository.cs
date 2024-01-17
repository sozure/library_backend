using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;
using VGManager.Library.Repositories.Interfaces.Boilerplate;

namespace VGManager.Library.Repositories.Boilerplate;

public abstract class SqlRepository<TEntity> : Repository<TEntity>, ISqlRepository<TEntity> where TEntity : class
{
    protected SqlRepository(DbContext dbContext)
        : base(dbContext)
    {
    }

    //
    // Summary:
    //     Add Entity without save changes async.
    //
    // Parameters:
    //   entity:
    //     The Entity to add to database.
    //
    //   cancellationToken:
    //     The System.Threading.CancellationToken.
    public ValueTask<EntityEntry<TEntity>> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        return _dbSet.AddAsync(entity, cancellationToken);
    }

    public async Task<TEntity[]> GetAllAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).Where(specification.Criteria).ToArrayAsync(cancellationToken);
    }

    public IAsyncEnumerable<TEntity> GetAllAsAsyncEnumerable(ISpecification<TEntity> specification)
    {
        return ApplySpecification(specification).Where(specification.Criteria).AsAsyncEnumerable();
    }

    //
    // Summary:
    //     Returns a new execution strategy
    //
    // Returns:
    //     The Microsoft.EntityFrameworkCore.Storage.ExecutionStrategy
    public virtual IExecutionStrategy GetExecutionStrategy()
    {
        return _dbContext.Database.CreateExecutionStrategy();
    }

    //
    // Summary:
    //     Call DbContext SaveChangesAsync method.
    //
    // Parameters:
    //   cancellationToken:
    //     The System.Threading.CancellationToken
    public virtual Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    //
    // Summary:
    //     Update Entity without save changes
    //
    // Parameters:
    //   entity:
    //     The entity to update in database.
    public EntityEntry<TEntity> Update(TEntity entity)
    {
        return _dbSet.Update(entity);
    }

    private IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> specification)
    {
        IQueryable<TEntity> seed = specification.Includes
            .Aggregate(_dbSet.AsQueryable(), (IQueryable<TEntity> current, Expression<Func<TEntity, object>> include) => current.Include(include));
        return specification.IncludeStrings.Aggregate(seed, (IQueryable<TEntity> current, string include) => current.Include(include));
    }
}
