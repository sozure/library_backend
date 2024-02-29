using VGManager.Library.Entities.VGEntities;
using VGManager.Library.Repositories.Interfaces.Boilerplate;

namespace VGManager.Library.Repositories.Interfaces.VGRepositories;

public interface IVGAddColdRepository : ISqlRepository<VGAddEntity>
{
    Task AddEntityAsync(VGAddEntity entity, CancellationToken cancellationToken = default);
    Task<IEnumerable<VGAddEntity>> GetAsync(
        string organization,
        string project,
        string user,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default
        );
    Task<IEnumerable<VGAddEntity>> GetAsync(
        string organization,
        string project,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default
        );
}
