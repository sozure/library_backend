using VGManager.Adapter.Models.Models;
using VGManager.Library.AzureAdapter.Entities;

namespace VGManager.Library.AzureAdapter.Interfaces;
public interface IProjectAdapter
{
    Task<AdapterResponseModel<IEnumerable<ProjectEntity>>> GetProjectsAsync(string baseUrl, string pat, CancellationToken cancellationToken = default);
}
