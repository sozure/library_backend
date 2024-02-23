using Microsoft.TeamFoundation.DistributedTask.WebApi;
using VGManager.Adapter.Models.Models;
using VGManager.Adapter.Models.Requests.VG;

namespace VGManager.Library.Services.Interfaces;

public interface IVariableGroupService
{
    Task<AdapterResponseModel<IEnumerable<VariableGroup>>> GetVariableGroupsAsync(
        VariableGroupModel variableGroupModel,
        IEnumerable<string>? potentialVariableGroups,
        bool containsKey,
        CancellationToken cancellationToken = default
        );
}
