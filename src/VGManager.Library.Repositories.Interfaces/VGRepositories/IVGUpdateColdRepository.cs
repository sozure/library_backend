using VGManager.Library.Entities.VGEntities;
using VGManager.Library.Repositories.Interfaces.Boilerplate;

namespace VGManager.Library.Repositories.Interfaces.VGRepositories;

public interface IVGUpdateColdRepository : ISqlRepository<VGUpdateEntity>
{
    Task AddEntityAsync(VGUpdateEntity entity, CancellationToken cancellationToken = default);
    Task<IEnumerable<VGUpdateEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<VGUpdateEntity>> GetAsync(
        string organization,
        string project,
        string user,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default
        );
    Task<IEnumerable<VGUpdateEntity>> GetAsync(
        string organization,
        string project,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default
        );
}
