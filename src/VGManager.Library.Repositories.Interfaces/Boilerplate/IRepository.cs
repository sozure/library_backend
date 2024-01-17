namespace VGManager.Library.Repositories.Interfaces.Boilerplate;

public interface IRepository<TEntity> where TEntity : class
{
    //
    // Summary:
    //     Add Entities and save changes async.
    //
    // Parameters:
    //   entities:
    //     Entities to add to database.
    //
    //   cancellationToken:
    //     The System.Threading.CancellationToken.
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    //
    // Summary:
    //     Find Entity by primary key(s) async.
    //
    // Parameters:
    //   keys:
    //     The primary key(s).
    //
    //   cancellationToken:
    //     The System.Threading.CancellationToken.
    //
    // Returns:
    //     An Entity if found, otherwise null.
    ValueTask<TEntity?> FindAsync(object[] keys, CancellationToken cancellationToken = default);

    //
    // Summary:
    //     Remove Entity and save changes.
    //
    // Parameters:
    //   entity:
    //     The entity to remove from database.
    void Remove(TEntity entity);

    //
    // Summary:
    //     Remove Entity and save changes.
    //
    // Parameters:
    //   entities:
    //     Entities to remove from database.
    void RemoveRange(IEnumerable<TEntity> entities);

    //
    // Summary:
    //     Update Entities and save changes.
    //
    // Parameters:
    //   entities:
    //     Entities to update in database.
    void UpdateRange(IEnumerable<TEntity> entities);
}
