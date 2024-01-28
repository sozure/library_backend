using VGManager.Adapter.Models.Models;
using VGManager.Adapter.Models.StatusEnums;
using VGManager.Library.Services.Models.VariableGroups.Requests;
using VGManager.Library.Services.Models.VariableGroups.Results;

namespace VGManager.Library.Services.Interfaces;

public interface IVariableService
{
    Task<AdapterStatus> UpdateVariableGroupsAsync(
        VariableGroupUpdateModel variableGroupUpdateModel,
        bool filterAsRegex,
        CancellationToken cancellationToken = default
        );

    Task<AdapterResponseModel<IEnumerable<VariableResult>>> GetVariablesAsync(
        VariableGroupModel variableGroupModel,
        CancellationToken cancellationToken = default
        );

    Task<AdapterStatus> AddVariablesAsync(
        VariableGroupAddModel variableGroupAddModel,
        CancellationToken cancellationToken = default
        );

    Task<AdapterStatus> DeleteVariablesAsync(
        VariableGroupModel variableGroupModel,
        bool filterAsRegex,
        CancellationToken cancellationToken = default
        );
}
