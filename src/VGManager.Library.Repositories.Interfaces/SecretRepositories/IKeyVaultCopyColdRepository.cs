using VGManager.Library.Entities.SecretEntities;
using VGManager.Library.Repositories.Interfaces.Boilerplate;

namespace VGManager.Library.Repositories.Interfaces.SecretRepositories;

public interface IKeyVaultCopyColdRepository : ISqlRepository<KeyVaultCopyEntity>
{
    Task AddEntityAsync(KeyVaultCopyEntity entity, CancellationToken cancellationToken = default);
    Task<IEnumerable<KeyVaultCopyEntity>> GetAsync(
        DateTime from,
        DateTime to,
        string user,
        CancellationToken cancellationToken = default
        );
    Task<IEnumerable<KeyVaultCopyEntity>> GetAsync(
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default
        );
}
