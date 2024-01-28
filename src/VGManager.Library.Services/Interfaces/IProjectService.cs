using VGManager.Adapter.Models.Models;
using VGManager.Adapter.Models.Requests;
using VGManager.Library.Services.Models.Common;

namespace VGManager.Library.Services.Interfaces;
public interface IProjectService
{
    Task<AdapterResponseModel<IEnumerable<ProjectRequest>>> GetProjectsAsync(BaseModel projectModel, CancellationToken cancellationToken = default);
}
