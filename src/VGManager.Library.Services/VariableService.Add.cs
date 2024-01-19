using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.DistributedTask.WebApi;
using System.Text.RegularExpressions;
using VGManager.Adapter.Models.Models;
using VGManager.Adapter.Models.StatusEnums;
using VGManager.Library.Entities.VGEntities;
using VGManager.Library.Services.Models.VariableGroups.Requests;

namespace VGManager.Library.Services;

public partial class VariableService
{
    public async Task<AdapterStatus> AddVariablesAsync(
        VariableGroupAddModel variableGroupAddModel,
        CancellationToken cancellationToken = default
        )
    {
        var vgEntity = await _variableGroupConnectionRepository.GetAllAsync(cancellationToken);
        var status = vgEntity.Status;

        if (status == AdapterStatus.Success)
        {
            var keyFilter = variableGroupAddModel.KeyFilter;
            var variableGroupFilter = variableGroupAddModel.VariableGroupFilter;
            var key = variableGroupAddModel.Key;
            var value = variableGroupAddModel.Value;
            var filteredVariableGroups = CollectVariableGroups(vgEntity, keyFilter, variableGroupFilter);

            var finalStatus = await AddVariablesAsync(filteredVariableGroups, key, value, cancellationToken);

            if (finalStatus == AdapterStatus.Success)
            {
                var org = variableGroupAddModel.Organization;
                var entity = new VGAddEntity
                {
                    VariableGroupFilter = variableGroupFilter,
                    Key = key,
                    Value = value,
                    Project = _project,
                    Organization = org,
                    User = variableGroupAddModel.UserName,
                    Date = DateTime.UtcNow
                };

                if (_organizationSettings.Organizations.Contains(org))
                {
                    await _additionColdRepository.AddEntityAsync(entity, cancellationToken);
                }
            }

            return finalStatus;
        }

        return status;
    }

    private IEnumerable<VariableGroup> CollectVariableGroups(
        AdapterResponseModel<IEnumerable<VariableGroup>> vgEntity,
        string? keyFilter,
        string variableGroupFilter
        )
    {
        IEnumerable<VariableGroup> filteredVariableGroups;
        if (keyFilter is not null)
        {
            try
            {
                var regex = new Regex(keyFilter.ToLower(), RegexOptions.None, TimeSpan.FromMilliseconds(5));

                filteredVariableGroups = _variableFilterService.FilterWithoutSecrets(true, variableGroupFilter, vgEntity.Data)
                .Select(vg => vg)
                .Where(vg => vg.Variables.Keys.ToList().FindAll(key => regex.IsMatch(key.ToLower())).Count == 0);
            }
            catch (RegexParseException ex)
            {
                _logger.LogError(ex, "Couldn't parse and create regex. Value: {value}.", keyFilter);
                filteredVariableGroups = _variableFilterService.FilterWithoutSecrets(true, variableGroupFilter, vgEntity.Data)
                .Select(vg => vg)
                .Where(vg => vg.Variables.Keys.ToList().FindAll(key => keyFilter.ToLower() == key.ToLower()).Count == 0);
            }
        }
        else
        {
            filteredVariableGroups = _variableFilterService.FilterWithoutSecrets(true, variableGroupFilter, vgEntity.Data);
        }

        return filteredVariableGroups;
    }

    private async Task<AdapterStatus> AddVariablesAsync(
        IEnumerable<VariableGroup> filteredVariableGroups,
        string key,
        string value,
        CancellationToken cancellationToken
        )
    {
        var updateCounter = 0;
        var counter = 0;
        foreach (var filteredVariableGroup in filteredVariableGroups)
        {
            counter++;
            try
            {
                var success = await AddVariableAsync(key, value, filteredVariableGroup, cancellationToken);

                if (success)
                {
                    updateCounter++;
                }
            }

            catch (ArgumentException ex)
            {
                _logger.LogDebug(
                    ex,
                    "Key has been added previously. Not a breaking error. Variable group: {variableGroupName}, Key: {key}",
                    filteredVariableGroup.Name,
                    key
                    );
                updateCounter++;
            }

            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Something went wrong during variable addition. Variable group: {variableGroupName}, Key: {key}",
                    filteredVariableGroup.Name,
                    key
                    );
            }
        }
        return updateCounter == counter ? AdapterStatus.Success : AdapterStatus.Unknown;
    }

    private async Task<bool> AddVariableAsync(string key, string value, VariableGroup filteredVariableGroup, CancellationToken cancellationToken)
    {
        var variableGroupName = filteredVariableGroup.Name;
        filteredVariableGroup.Variables.Add(key, value);
        var variableGroupParameters = GetVariableGroupParameters(filteredVariableGroup, variableGroupName);

        var updateStatus = await _variableGroupConnectionRepository.UpdateAsync(
            variableGroupParameters,
            filteredVariableGroup.Id,
            cancellationToken
            );

        if (updateStatus == AdapterStatus.Success)
        {
            return true;
        }

        return false;
    }
}
