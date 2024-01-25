
using Microsoft.TeamFoundation.DistributedTask.WebApi;
using VGManager.Adapter.Models.StatusEnums;
using VGManager.Library.Entities.VGEntities;
using VGManager.Library.Services.Models.VariableGroups.Requests;

namespace VGManager.Library.Services;

public partial class VariableService
{
    public async Task<AdapterStatus> DeleteVariablesAsync(
        VariableGroupModel variableGroupModel,
        bool filterAsRegex,
        CancellationToken cancellationToken = default
        )
    {
        var vgEntity = await GetAllAsync(variableGroupModel, cancellationToken);
        var status = vgEntity.Status;

        if (status == AdapterStatus.Success)
        {
            var variableGroupFilter = variableGroupModel.VariableGroupFilter;
            var filteredVariableGroups = _variableFilterService.FilterWithoutSecrets(filterAsRegex, variableGroupFilter, vgEntity.Data);
            var finalStatus = await DeleteVariablesAsync(variableGroupModel, filteredVariableGroups, cancellationToken);
            if (finalStatus == AdapterStatus.Success)
            {
                var org = variableGroupModel.Organization;
                var entity = new VGDeleteEntity
                {
                    VariableGroupFilter = variableGroupFilter,
                    Key = variableGroupModel.KeyFilter,
                    Project = variableGroupModel.Project,
                    Organization = org,
                    User = variableGroupModel.UserName,
                    Date = DateTime.UtcNow
                };

                if (_organizationSettings.Organizations.Contains(org))
                {
                    await _deletionColdRepository.AddEntityAsync(entity, cancellationToken);
                }
            }
            return finalStatus;
        }

        return status;
    }

    private async Task<AdapterStatus> DeleteVariablesAsync(
        VariableGroupModel variableGroupModel,
        IEnumerable<VariableGroup> filteredVariableGroups,
        CancellationToken cancellationToken
        )
    {
        var deletionCounter1 = 0;
        var deletionCounter2 = 0;
        var keyFilter = variableGroupModel.KeyFilter;

        foreach (var filteredVariableGroup in filteredVariableGroups)
        {
            var variableGroupName = filteredVariableGroup.Name;

            var deleteIsNeeded = DeleteVariables(
                filteredVariableGroup,
                keyFilter,
                variableGroupModel.ValueFilter
                );

            if (deleteIsNeeded)
            {
                deletionCounter1++;
                var variableGroupParameters = GetVariableGroupParameters(filteredVariableGroup, variableGroupName);

                var updateStatus = await UpdateAsync(
                    variableGroupModel,
                    variableGroupParameters,
                    filteredVariableGroup.Id,
                    cancellationToken
                    );

                if (updateStatus == AdapterStatus.Success)
                {
                    deletionCounter2++;
                }
            }
        }
        return deletionCounter1 == deletionCounter2 ? AdapterStatus.Success : AdapterStatus.Unknown;
    }

    private bool DeleteVariables(VariableGroup filteredVariableGroup, string keyFilter, string? valueCondition)
    {
        var deleteIsNeeded = false;
        var filteredVariables = _variableFilterService.Filter(filteredVariableGroup.Variables, keyFilter);
        foreach (var filteredVariable in filteredVariables)
        {
            var variableValue = filteredVariable.Value.Value;

            if (valueCondition is not null)
            {
                if (valueCondition.Equals(variableValue))
                {
                    filteredVariableGroup.Variables.Remove(filteredVariable.Key);
                    deleteIsNeeded = true;
                }
            }
            else
            {
                filteredVariableGroup.Variables.Remove(filteredVariable.Key);
                deleteIsNeeded = true;
            }
        }

        return deleteIsNeeded;
    }
}
