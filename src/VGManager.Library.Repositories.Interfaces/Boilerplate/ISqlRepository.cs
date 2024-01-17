using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;

namespace VGManager.Library.Repositories.Interfaces.Boilerplate;

public interface ISqlRepository<TEntity> : IRepository<TEntity> where TEntity : class
{
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
    ValueTask<EntityEntry<TEntity>> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    //
    // Summary:
    //     Returns a new execution strategy
    //
    // Returns:
    //     The Microsoft.EntityFrameworkCore.Storage.ExecutionStrategy
    IExecutionStrategy GetExecutionStrategy();

    Task<TEntity[]> GetAllAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);

    IAsyncEnumerable<TEntity> GetAllAsAsyncEnumerable(ISpecification<TEntity> specification);

    //
    // Summary:
    //     Call DbContext SaveChangesAsync method.
    //
    // Parameters:
    //   cancellationToken:
    //     The System.Threading.CancellationToken
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    //
    // Summary:
    //     Update Entity without save changes
    //
    // Parameters:
    //   entity:
    //     The entity to update in database.
    EntityEntry<TEntity> Update(TEntity entity);
}
