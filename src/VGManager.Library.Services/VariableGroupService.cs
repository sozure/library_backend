using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.DistributedTask.WebApi;
using System.Text.Json;
using VGManager.Adapter.Azure.Services.Requests;
using VGManager.Adapter.Models.Kafka;
using VGManager.Adapter.Models.Models;
using VGManager.Adapter.Models.Response;
using VGManager.Adapter.Models.StatusEnums;
using VGManager.Library.Services.Interfaces;
using VGManager.Library.Services.Models.VariableGroups.Requests;

namespace VGManager.Library.Services;

public class VariableGroupService : IVariableGroupService
{
    private readonly IAdapterCommunicator _adapterCommunicator;
    private readonly IVariableFilterService _variableFilterService;
    private readonly ILogger _logger;

    public VariableGroupService(
        IAdapterCommunicator adapterCommunicator,
        IVariableFilterService variableFilterService,
        ILogger<VariableGroupService> logger
        )
    {
        _adapterCommunicator = adapterCommunicator;
        _variableFilterService = variableFilterService;
        _logger = logger;
    }

    public async Task<AdapterResponseModel<IEnumerable<VariableGroup>>> GetVariableGroupsAsync(
        VariableGroupModel variableGroupModel,
        bool containsKey,
        CancellationToken cancellationToken = default
        )
    {
        _logger.LogInformation("Get variable groups from {project} Azure project.", variableGroupModel.Project);

        var request = new ExtendedBaseRequest()
        {
            Organization = variableGroupModel.Organization,
            PAT = variableGroupModel.PAT,
            Project = variableGroupModel.Project
        };

        (var isSuccess, var response) = await _adapterCommunicator.CommunicateWithAdapterAsync(
            request,
            CommandTypes.GetAllVGRequest,
            cancellationToken
            );

        if (!isSuccess)
        {
            return new() { Data = Enumerable.Empty<VariableGroup>() };
        }

        var adapterResult = JsonSerializer.Deserialize<BaseResponse<AdapterResponseModel<IEnumerable<VariableGroup>>>>(response)?.Data;

        if (adapterResult is null)
        {
            return new() { Data = Enumerable.Empty<VariableGroup>() };
        }

        var status = adapterResult.Status;

        if (status == AdapterStatus.Success)
        {
            var filteredVariableGroups = variableGroupModel.ContainsSecrets ?
                        _variableFilterService.Filter(adapterResult.Data, variableGroupModel.VariableGroupFilter) :
                        _variableFilterService.FilterWithoutSecrets(true, variableGroupModel.VariableGroupFilter, adapterResult.Data);

            var result = GetVariableGroups(filteredVariableGroups, variableGroupModel.KeyFilter, containsKey);
            return GetResult(status, result);
        }
        else
        {
            return GetResult(status, Enumerable.Empty<VariableGroup>());
        }
    }

    private static IEnumerable<VariableGroup> GetVariableGroups(IEnumerable<VariableGroup> filteredVariableGroups, string keyFilter, bool containsKey)
    {
        var result = new List<VariableGroup>();
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

    private static AdapterResponseModel<IEnumerable<VariableGroup>> GetResult(AdapterStatus status, IEnumerable<VariableGroup> variableGroups)
        => new()
        {
            Status = status,
            Data = variableGroups
        };
}
