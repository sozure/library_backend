using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.DistributedTask.WebApi;
using VGManager.Library.AzureAdapter.Interfaces;
using VGManager.Library.Models.Models;
using VGManager.Library.Models.StatusEnums;
using VGManager.Library.Services.Interfaces;
using VGManager.Library.Services.Models.VariableGroups.Requests;

namespace VGManager.Library.Services;

public class VariableGroupService : IVariableGroupService
{
    private readonly IVariableFilterService _variableFilterService;
    private readonly IVariableGroupAdapter _variableGroupConnectionRepository;
    private readonly ILogger _logger;

    public VariableGroupService(
        IVariableFilterService variableFilterService,
        IVariableGroupAdapter variableGroupConnectionRepository,
        ILogger<VariableGroupService> logger
        )
    {
        _variableFilterService = variableFilterService;
        _variableGroupConnectionRepository = variableGroupConnectionRepository;
        _logger = logger;
    }

    public async Task<AdapterResponseModel<IEnumerable<VariableGroup>>> GetVariableGroupsAsync(
        VariableGroupModel variableGroupModel,
        bool containsKey,
        CancellationToken cancellationToken = default
        )
    {
        _logger.LogInformation("Get variable groups from {project} Azure project.", variableGroupModel.Project);
        var vgEntity = await _variableGroupConnectionRepository.GetAllAsync(cancellationToken);
        var status = vgEntity.Status;

        if (status == AdapterStatus.Success)
        {
            var filteredVariableGroups = variableGroupModel.ContainsSecrets ?
                        _variableFilterService.Filter(vgEntity.Data, variableGroupModel.VariableGroupFilter) :
                        _variableFilterService.FilterWithoutSecrets(true, variableGroupModel.VariableGroupFilter, vgEntity.Data);

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
