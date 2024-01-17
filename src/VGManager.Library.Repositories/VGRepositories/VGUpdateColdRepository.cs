using VGManager.Library.Entities.VGEntities;
using VGManager.Library.Repositories.Boilerplate;
using VGManager.Library.Repositories.DbContexts;
using VGManager.Library.Repositories.Interfaces.VGRepositories;

namespace VGManager.Library.Repositories.VGRepositories;

public class VGUpdateColdRepository : SqlRepository<VGUpdateEntity>, IVGUpdateColdRepository
{
    public VGUpdateColdRepository(OperationsDbContext dbContext) : base(dbContext)
    {
    }

    public async Task AddEntityAsync(VGUpdateEntity entity, CancellationToken cancellationToken = default)
    {
        await AddAsync(entity, cancellationToken);
        await SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<VGUpdateEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var result = await GetAllAsync(new EditionSpecification(), cancellationToken);
        return result?.ToList() ?? Enumerable.Empty<VGUpdateEntity>();
    }

    public async Task<IEnumerable<VGUpdateEntity>> GetAsync(
        string organization,
        string project,
        string user,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default
        )
    {
        var result = await GetAllAsync(new EditionSpecification(organization, project, user, from, to.AddDays(1)), cancellationToken);
        return result?.ToList() ?? Enumerable.Empty<VGUpdateEntity>();
    }

    public async Task<IEnumerable<VGUpdateEntity>> GetAsync(
        string organization,
        string project,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default
        )
    {
        var result = await GetAllAsync(new EditionSpecification(organization, project, from, to.AddDays(1)), cancellationToken);
        return result?.ToList() ?? Enumerable.Empty<VGUpdateEntity>();
    }

    public class EditionSpecification : SpecificationBase<VGUpdateEntity>
    {
        public EditionSpecification() : base(editionEntity => !string.IsNullOrEmpty(editionEntity.Id))
        {
        }

        public EditionSpecification(string organization, string project, string user, DateTime from, DateTime to) : base(
            editionEntity => editionEntity.Date >= from &&
            editionEntity.Date <= to &&
            editionEntity.Organization == organization &&
            editionEntity.Project == project &&
            editionEntity.User.Contains(user)
            )
        {
        }

        public EditionSpecification(string organization, string project, DateTime from, DateTime to) : base(
            editionEntity => editionEntity.Date >= from &&
            editionEntity.Date <= to &&
            editionEntity.Organization == organization &&
            editionEntity.Project == project
            )
        {
        }
    }
}
