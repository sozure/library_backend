using VGManager.Library.Models.Models;
using VGManager.Library.Services.Models.Common;
using VGManager.Library.Services.Models.Projects;

namespace VGManager.Library.Services.Interfaces;
public interface IProjectService
{
    Task<AdapterResponseModel<IEnumerable<ProjectResult>>> GetProjectsAsync(BaseModel projectModel, CancellationToken cancellationToken = default);
}
