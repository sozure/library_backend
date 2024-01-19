using Microsoft.TeamFoundation.DistributedTask.WebApi;
using VGManager.Adapter.Models.Models;
using VGManager.Library.Services.Models.VariableGroups.Requests;

namespace VGManager.Library.Services.Interfaces;

public interface IVariableGroupService
{
    Task<AdapterResponseModel<IEnumerable<VariableGroup>>> GetVariableGroupsAsync(
        VariableGroupModel variableGroupModel,
        bool containsKey,
        CancellationToken cancellationToken = default
        );
}
