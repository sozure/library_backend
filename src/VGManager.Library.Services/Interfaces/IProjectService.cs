using VGManager.Adapter.Models.Models;
using VGManager.Adapter.Models.Requests;
using VGManager.Adapter.Models.Requests.VG;

namespace VGManager.Library.Services.Interfaces;
public interface IProjectService
{
    Task<AdapterResponseModel<IEnumerable<ProjectRequest>>> GetProjectsAsync(BaseModel projectModel, CancellationToken cancellationToken = default);
}
