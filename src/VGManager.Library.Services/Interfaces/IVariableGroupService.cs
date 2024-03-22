using VGManager.Adapter.Models.Models;
using VGManager.Adapter.Models.Requests.VG;
using VGManager.Adapter.Models.Response;

namespace VGManager.Library.Services.Interfaces;

public interface IVariableGroupService
{
    Task<AdapterResponseModel<IEnumerable<SimplifiedVGResponse<string>>>> GetVariableGroupsAsync(
        VariableGroupModel variableGroupModel,
        IEnumerable<string>? potentialVariableGroups,
        bool containsKey,
        CancellationToken cancellationToken = default
        );
}
