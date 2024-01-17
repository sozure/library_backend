using VGManager.Library.Models.Models;
using VGManager.Library.Models.StatusEnums;
using VGManager.Library.Services.Models.VariableGroups.Requests;
using VGManager.Library.Services.Models.VariableGroups.Results;

namespace VGManager.Library.Services.Interfaces;

public interface IVariableService
{
    void SetupConnectionRepository(VariableGroupModel variableGroupModel);

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
