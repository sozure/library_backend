using VGManager.Library.Entities.VGEntities;
using VGManager.Library.Repositories.Boilerplate;
using VGManager.Library.Repositories.DbContexts;
using VGManager.Library.Repositories.Interfaces.VGRepositories;

namespace VGManager.Library.Repositories.VGRepositories;

public class VGDeleteColdRepository(OperationsDbContext dbContext) : SqlRepository<VGDeleteEntity>(dbContext), IVGDeleteColdRepository
{
    public async Task AddEntityAsync(VGDeleteEntity entity, CancellationToken cancellationToken = default)
    {
        await AddAsync(entity, cancellationToken);
        await SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<VGDeleteEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var result = await GetAllAsync(new DeletionSpecification(), cancellationToken);
        return result?.ToList() ?? Enumerable.Empty<VGDeleteEntity>();
    }

    public async Task<IEnumerable<VGDeleteEntity>> GetAsync(
        string organization,
        string project,
        string user,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default
        )
    {
        var result = await GetAllAsync(new DeletionSpecification(organization, project, user, from, to.AddDays(1)), cancellationToken);
        return result?.ToList() ?? Enumerable.Empty<VGDeleteEntity>();
    }

    public async Task<IEnumerable<VGDeleteEntity>> GetAsync(
        string organization,
        string project,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default
        )
    {
        var result = await GetAllAsync(new DeletionSpecification(organization, project, from, to.AddDays(1)), cancellationToken);
        return result?.ToList() ?? Enumerable.Empty<VGDeleteEntity>();
    }

    public class DeletionSpecification : SpecificationBase<VGDeleteEntity>
    {
        public DeletionSpecification() : base(deletionEntity => !string.IsNullOrEmpty(deletionEntity.Id))
        {
        }

        public DeletionSpecification(string organization, string project, string user, DateTime from, DateTime to) : base(
            deletionEntity => deletionEntity.Date >= from &&
            deletionEntity.Date <= to &&
            deletionEntity.Organization == organization &&
            deletionEntity.Project == project &&
            deletionEntity.User.Contains(user)
            )
        {
        }

        public DeletionSpecification(string organization, string project, DateTime from, DateTime to) : base(
            deletionEntity => deletionEntity.Date >= from &&
            deletionEntity.Date <= to &&
            deletionEntity.Organization == organization &&
            deletionEntity.Project == project
            )
        {
        }
    }
}
