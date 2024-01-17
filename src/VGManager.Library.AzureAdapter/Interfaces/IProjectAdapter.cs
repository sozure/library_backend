using VGManager.Library.AzureAdapter.Entities;
using VGManager.Library.Models.Models;

namespace VGManager.Library.AzureAdapter.Interfaces;
public interface IProjectAdapter
{
    Task<AdapterResponseModel<IEnumerable<ProjectEntity>>> GetProjectsAsync(string baseUrl, string pat, CancellationToken cancellationToken = default);
}
