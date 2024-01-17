using VGManager.Library.Entities.SecretEntities;
using VGManager.Library.Repositories.Boilerplate;
using VGManager.Library.Repositories.DbContexts;
using VGManager.Library.Repositories.Interfaces.SecretRepositories;

namespace VGManager.Library.Repositories.SecretRepositories;

public class SecretChangeColdRepository : SqlRepository<SecretChangeEntity>, ISecretChangeColdRepository
{
    public SecretChangeColdRepository(OperationsDbContext dbContext) : base(dbContext)
    {
    }

    public async Task AddEntityAsync(SecretChangeEntity entity, CancellationToken cancellationToken = default)
    {
        await AddAsync(entity, cancellationToken);
        await SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<SecretChangeEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var result = await GetAllAsync(new SecretChangeSpecification(), cancellationToken);
        return result?.ToList() ?? Enumerable.Empty<SecretChangeEntity>();
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
        return result?.ToList() ?? Enumerable.Empty<SecretChangeEntity>();
    }

    public async Task<IEnumerable<SecretChangeEntity>> GetAsync(
        DateTime from,
        DateTime to,
        string keyVaultName,
        CancellationToken cancellationToken = default
        )
    {
        var result = await GetAllAsync(new SecretChangeSpecification(from, to.AddDays(1), keyVaultName), cancellationToken);
        return result?.ToList() ?? Enumerable.Empty<SecretChangeEntity>();
    }

    public class SecretChangeSpecification : SpecificationBase<SecretChangeEntity>
    {
        public SecretChangeSpecification() : base(entity => !string.IsNullOrEmpty(entity.Id))
        {
        }

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
