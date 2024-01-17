using Microsoft.TeamFoundation.DistributedTask.WebApi;
using VGManager.Library.Models.Models;
using VGManager.Library.Models.StatusEnums;

namespace VGManager.Library.AzureAdapter.Interfaces;

public interface IVariableGroupAdapter
{
    void Setup(string organization, string project, string pat);
    Task<AdapterResponseModel<IEnumerable<VariableGroup>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<AdapterStatus> UpdateAsync(VariableGroupParameters variableGroupParameters, int variableGroupId, CancellationToken cancellationToken = default);
}
