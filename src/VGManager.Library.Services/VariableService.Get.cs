using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using VGManager.Adapter.Models.Models;
using VGManager.Adapter.Models.Requests.VG;
using VGManager.Adapter.Models.Response;
using VGManager.Adapter.Models.StatusEnums;
using VGManager.Library.Services.Models.VariableGroups.Results;

namespace VGManager.Library.Services;

public partial class VariableService
{
    private AdapterResponseModel<IEnumerable<VariableResult>> GetVariablesAsync(
        VariableGroupModel variableGroupModel,
        AdapterResponseModel<IEnumerable<SimplifiedVGResponse<string>>> vgEntity,
        AdapterStatus status
        )
    {
        var matchedVariables = new List<VariableResult>();
        var valueFilter = variableGroupModel.ValueFilter;
        var keyFilter = variableGroupModel.KeyFilter;
        Regex? valueRegex = null;

        if (valueFilter is not null)
        {
            try
            {
                valueRegex = new Regex(valueFilter.ToLower(), RegexOptions.None, TimeSpan.FromMilliseconds(5));
            }
            catch (RegexParseException ex)
            {
                logger.LogError(ex, "Couldn't parse and create regex. Value: {value}.", valueFilter);
                return new()
                {
                    Status = status,
                    Data = matchedVariables,
                };
            }
        }

        if (variableGroupModel.KeyIsRegex ?? false)
        {
            Regex keyRegex;
            try
            {
                keyRegex = new Regex(keyFilter.ToLower(), RegexOptions.None, TimeSpan.FromMilliseconds(5));
            }
            catch (RegexParseException ex)
            {
                logger.LogError(ex, "Couldn't parse and create regex. Value: {value}.", keyFilter);
                return new()
                {
                    Status = status,
                    Data = matchedVariables,
                };
            }

            foreach (var filteredVariableGroup in vgEntity.Data)
            {
                matchedVariables.AddRange(
                    GetVariables(keyRegex, valueRegex, variableGroupModel.Project, filteredVariableGroup)
                    );
            }
        }
        else
        {
            foreach (var filteredVariableGroup in vgEntity.Data)
            {
                matchedVariables.AddRange(
                    GetVariables(keyFilter, valueRegex, variableGroupModel.Project, filteredVariableGroup)
                    );
            }
        }

        return new()
        {
            Status = status,
            Data = matchedVariables,
        };
    }

    private List<VariableResult> GetVariables(
        string keyFilter,
        Regex? valueRegex,
        string project,
        SimplifiedVGResponse<string> filteredVariableGroup
        )
    {
        var filteredVariables = variableFilterService.Filter(filteredVariableGroup.Variables, keyFilter);
        return CollectVariables(valueRegex, filteredVariableGroup, project, filteredVariables);
    }

    private List<VariableResult> GetVariables(
        Regex keyRegex,
        Regex? valueRegex,
        string project,
        SimplifiedVGResponse<string> filteredVariableGroup
        )
    {
        var filteredVariables = variableFilterService.Filter(filteredVariableGroup.Variables, keyRegex);
        return CollectVariables(valueRegex, filteredVariableGroup, project, filteredVariables);
    }

    private List<VariableResult> CollectVariables(
        Regex? valueRegex,
        SimplifiedVGResponse<string> filteredVariableGroup,
        string project,
        IEnumerable<KeyValuePair<string, string>> filteredVariables
        )
    {
        var result = new List<VariableResult>();
        foreach (var filteredVariable in filteredVariables)
        {
            var variableValue = filteredVariable.Value ?? string.Empty;
            if (valueRegex is not null)
            {
                if (valueRegex.IsMatch(variableValue.ToLower()))
                {
                    result.AddRange(
                        AddVariableResult(filteredVariableGroup, filteredVariable, variableValue, project)
                        );
                }
            }
            else
            {
                result.AddRange(
                    AddVariableResult(filteredVariableGroup, filteredVariable, variableValue, project)
                    );
            }
        }
        return result;
    }

    private List<VariableResult> AddVariableResult(
        SimplifiedVGResponse<string> filteredVariableGroup,
        KeyValuePair<string, string> filteredVariable,
        string variableValue,
        string project
        )
    {
        var subResult = new List<VariableResult>();
        if (filteredVariableGroup.Type == SecretVGType)
        {
            subResult.Add(new VariableResult()
            {
                Project = project,
                SecretVariableGroup = true,
                VariableGroupName = filteredVariableGroup.Name,
                VariableGroupKey = filteredVariable.Key,
                KeyVaultName = filteredVariableGroup.KeyVaultName ?? string.Empty
            });
        }
        else
        {
            subResult.Add(new VariableResult()
            {
                Project = project,
                SecretVariableGroup = false,
                VariableGroupName = filteredVariableGroup.Name,
                VariableGroupKey = filteredVariable.Key,
                VariableGroupValue = variableValue
            });
        }
        return subResult;
    }
}
