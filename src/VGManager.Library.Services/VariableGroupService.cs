using Microsoft.Extensions.Logging;
using System.Text.Json;
using VGManager.Adapter.Models.Kafka;
using VGManager.Adapter.Models.Models;
using VGManager.Adapter.Models.Requests.VG;
using VGManager.Adapter.Models.Response;
using VGManager.Adapter.Models.StatusEnums;
using VGManager.Library.Services.Interfaces;


namespace VGManager.Library.Services;

public class VariableGroupService(
    IAdapterCommunicator adapterCommunicator,
    ILogger<VariableGroupService> logger
        ) : IVariableGroupService
{
    public async Task<AdapterResponseModel<IEnumerable<SimplifiedVGResponse<string>>>> GetVariableGroupsAsync(
        VariableGroupModel variableGroupModel,
        IEnumerable<string>? potentialVariableGroups,
        bool containsKey,
        CancellationToken cancellationToken = default
        )
    {
        logger.LogInformation("Get variable groups from {project} Azure project.", variableGroupModel.Project);

        var request = new GetVGRequest()
        {
            Organization = variableGroupModel.Organization,
            PAT = variableGroupModel.PAT,
            Project = variableGroupModel.Project,
            ContainsSecrets = variableGroupModel.ContainsSecrets,
            VariableGroupFilter = variableGroupModel.VariableGroupFilter,
            FilterAsRegex = true,
            PotentialVariableGroups = potentialVariableGroups?.ToArray(),
            KeyIsRegex = variableGroupModel.KeyIsRegex,
            KeyFilter = variableGroupModel.KeyFilter,
        };

        (var isSuccess, var response) = await adapterCommunicator.CommunicateWithAdapterAsync(
            request,
            CommandTypes.GetAllVGRequest,
            cancellationToken
            );

        if (!isSuccess)
        {
            return new() { Data = [] };
        }

        var adapterResult = JsonSerializer.Deserialize<BaseResponse<AdapterResponseModel<IEnumerable<SimplifiedVGResponse<string>>>>>(response)?.Data;

        if (adapterResult is null)
        {
            return new() { Data = [] };
        }

        var status = adapterResult.Status;

        if (status == AdapterStatus.Success)
        {
            var result = GetVariableGroups(adapterResult.Data, variableGroupModel.KeyFilter, containsKey);
            return GetResult(status, result);
        }
        else
        {
            return GetResult(status, []);
        }
    }

    private static List<SimplifiedVGResponse<string>> GetVariableGroups(
        IEnumerable<SimplifiedVGResponse<string>> filteredVariableGroups,
        string keyFilter,
        bool containsKey
        )
    {
        var result = new List<SimplifiedVGResponse<string>>();
        foreach (var variableGroup in filteredVariableGroups)
        {
            if (containsKey)
            {
                if (variableGroup.Variables.ContainsKey(keyFilter))
                {
                    result.Add(variableGroup);
                }
            }
            else
            {
                if (!variableGroup.Variables.ContainsKey(keyFilter))
                {
                    result.Add(variableGroup);
                }
            }
        }
        return result;
    }

    private static AdapterResponseModel<IEnumerable<SimplifiedVGResponse<string>>> GetResult(
        AdapterStatus status,
        IEnumerable<SimplifiedVGResponse<string>> variableGroups
        )
        => new()
        {
            Status = status,
            Data = variableGroups
        };
}
