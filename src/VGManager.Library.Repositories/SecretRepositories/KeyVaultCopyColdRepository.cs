using VGManager.Library.Entities.SecretEntities;
using VGManager.Library.Repositories.Boilerplate;
using VGManager.Library.Repositories.DbContexts;
using VGManager.Library.Repositories.Interfaces.SecretRepositories;

namespace VGManager.Library.Repositories.SecretRepositories;

public class KeyVaultCopyColdRepository(OperationsDbContext dbContext) : SqlRepository<KeyVaultCopyEntity>(dbContext), IKeyVaultCopyColdRepository
{
    public async Task AddEntityAsync(KeyVaultCopyEntity entity, CancellationToken cancellationToken = default)
    {
        await AddAsync(entity, cancellationToken);
        await SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<KeyVaultCopyEntity>> GetAsync(
        DateTime from,
        DateTime to,
        string user,
        CancellationToken cancellationToken = default
        )
    {
        var result = await GetAllAsync(new KeyVaultCopySpecification(from, to.AddDays(1), user), cancellationToken);
        return result?.ToList() ?? [];
    }

    public async Task<IEnumerable<KeyVaultCopyEntity>> GetAsync(
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default
        )
    {
        var result = await GetAllAsync(new KeyVaultCopySpecification(from, to.AddDays(1)), cancellationToken);
        return result?.ToList() ?? [];
    }

    public class KeyVaultCopySpecification : SpecificationBase<KeyVaultCopyEntity>
    {
        public KeyVaultCopySpecification(DateTime from, DateTime to, string user) : base(
            entity => entity.Date >= from &&
            entity.Date <= to &&
            entity.User == user
            )
        {
        }

        public KeyVaultCopySpecification(DateTime from, DateTime to) : base(
            entity => entity.Date >= from &&
            entity.Date <= to
            )
        {
        }
    }
}
