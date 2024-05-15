using VGManager.Library.Entities.SecretEntities;
using VGManager.Library.Repositories.Boilerplate;
using VGManager.Library.Repositories.DbContexts;
using VGManager.Library.Repositories.Interfaces.SecretRepositories;

namespace VGManager.Library.Repositories.SecretRepositories;

public class SecretChangeColdRepository(OperationsDbContext dbContext) : SqlRepository<SecretChangeEntity>(dbContext), ISecretChangeColdRepository
{
    public async Task AddEntityAsync(SecretChangeEntity entity, CancellationToken cancellationToken = default)
    {
        await AddAsync(entity, cancellationToken);
        await SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<SecretChangeEntity>> GetAsync(
        DateTime from,
        DateTime to,
        string user,
        string keyVaultName,
        CancellationToken cancellationToken = default
        )
    {
        var result = await GetAllAsync(new SecretChangeSpecification(from, to.AddDays(1), user, keyVaultName), cancellationToken);
        return result?.ToList() ?? [];
    }

    public async Task<IEnumerable<SecretChangeEntity>> GetAsync(
        DateTime from,
        DateTime to,
        string keyVaultName,
        CancellationToken cancellationToken = default
        )
    {
        var result = await GetAllAsync(new SecretChangeSpecification(from, to.AddDays(1), keyVaultName), cancellationToken);
        return result?.ToList() ?? [];
    }

    public class SecretChangeSpecification : SpecificationBase<SecretChangeEntity>
    {
        public SecretChangeSpecification(DateTime from, DateTime to, string user, string keyVaultName) : base(
            secretChange => secretChange.Date >= from &&
            secretChange.Date <= to &&
            secretChange.User == user &&
            secretChange.KeyVaultName == keyVaultName
            )
        {
        }

        public SecretChangeSpecification(DateTime from, DateTime to, string keyVaultName) : base(
            secretChange => secretChange.Date >= from &&
            secretChange.Date <= to &&
            secretChange.KeyVaultName == keyVaultName
            )
        {
        }
    }
}
