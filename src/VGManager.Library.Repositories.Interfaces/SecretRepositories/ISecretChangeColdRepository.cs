using VGManager.Library.Entities.SecretEntities;
using VGManager.Library.Repositories.Interfaces.Boilerplate;

namespace VGManager.Library.Repositories.Interfaces.SecretRepositories;

public interface ISecretChangeColdRepository : ISqlRepository<SecretChangeEntity>
{
    Task AddEntityAsync(SecretChangeEntity entity, CancellationToken cancellationToken = default);
    Task<IEnumerable<SecretChangeEntity>> GetAsync(
        DateTime from,
        DateTime to,
        string keyVaultName,
        CancellationToken cancellationToken = default
        );
    Task<IEnumerable<SecretChangeEntity>> GetAsync(
        DateTime from,
        DateTime to,
        string user,
        string keyVaultName,
        CancellationToken cancellationToken = default
        );
}
