using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.DistributedTask.WebApi;
using System.Text.Json;
using VGManager.Adapter.Models.Kafka;
using VGManager.Adapter.Models.Models;
using VGManager.Adapter.Models.Requests;
using VGManager.Adapter.Models.Response;
using VGManager.Adapter.Models.StatusEnums;
using VGManager.Library.Repositories.Interfaces.VGRepositories;
using VGManager.Library.Services.Interfaces;
using VGManager.Library.Services.Models;
using VGManager.Library.Services.Models.VariableGroups.Requests;
using VGManager.Library.Services.Settings;

namespace VGManager.Library.Services;

public partial class VariableService : IVariableService
{
    private readonly IAdapterCommunicator _adapterCommunicator;
    private readonly IVGAddColdRepository _additionColdRepository;
    private readonly IVGDeleteColdRepository _deletionColdRepository;
    private readonly IVGUpdateColdRepository _editionColdRepository;
    private readonly IVariableFilterService _variableFilterService;
    private readonly OrganizationSettings _organizationSettings;
    private readonly ILogger _logger;

    private readonly string SecretVGType = "AzureKeyVault";

    public VariableService(
        IAdapterCommunicator adapterCommunicator,
        IVGAddColdRepository additionColdRepository,
        IVGDeleteColdRepository deletedColdRepository,
        IVGUpdateColdRepository editionColdRepository,
        IVariableFilterService variableFilterService,
        IOptions<OrganizationSettings> organizationSettings,
        ILogger<VariableService> logger
        )
    {
        _adapterCommunicator = adapterCommunicator;
        _additionColdRepository = additionColdRepository;
        _deletionColdRepository = deletedColdRepository;
        _editionColdRepository = editionColdRepository;
        _variableFilterService = variableFilterService;
        _organizationSettings = organizationSettings.Value;
        _logger = logger;
    }

    private async Task<AdapterResponseModel<IEnumerable<SimplifiedVGResponse>>> GetAllAsync(
        VariableGroupModel variableGroupModel,
        bool filterAsRegex,
        CancellationToken cancellationToken
        )
    {
        var request = new GetVGRequest()
        {
            Organization = variableGroupModel.Organization,
            PAT = variableGroupModel.PAT,
            Project = variableGroupModel.Project,
            ContainsSecrets = variableGroupModel.ContainsSecrets,
            VariableGroupFilter = variableGroupModel.VariableGroupFilter,
            FilterAsRegex = filterAsRegex,
            KeyIsRegex = variableGroupModel.KeyIsRegex,
            KeyFilter = variableGroupModel.KeyFilter,
        };

        (var isSuccess, var response) = await _adapterCommunicator.CommunicateWithAdapterAsync(
            request,
            CommandTypes.GetAllVGRequest,
            cancellationToken
            );

        if (!isSuccess)
        {
            return new() { Data = Enumerable.Empty<SimplifiedVGResponse>() };
        }

        var adapterResult = JsonSerializer.Deserialize<BaseResponse<AdapterResponseModel<IEnumerable<SimplifiedVGResponse>>>>(response)?.Data;

        if (adapterResult is null)
        {
            return new() { Data = Enumerable.Empty<SimplifiedVGResponse>() };
        }

        return adapterResult;
    }

    private async Task<AdapterStatus> UpdateAsync(
        VariableGroupModel variableGroupModel,
        VariableGroupParameters variableGroupParameters,
        int variableGroupId,
        CancellationToken cancellationToken
        )
    {
        var request = new UpdateVGRequest()
        {
            Organization = variableGroupModel.Organization,
            PAT = variableGroupModel.PAT,
            Project = variableGroupModel.Project,
            VariableGroupId = variableGroupId,
            Params = variableGroupParameters
        };

        (var isSuccess, var response) = await _adapterCommunicator.CommunicateWithAdapterAsync(
            request,
            CommandTypes.UpdateVGRequest,
            cancellationToken
            );

        if (!isSuccess)
        {
            return AdapterStatus.Unknown;
        }

        var adapterResult = JsonSerializer.Deserialize<BaseResponse<AdapterStatus>>(response)?.Data;
        return adapterResult ?? AdapterStatus.Unknown;
    }

    private static VariableGroupParameters GetVariableGroupParameters(SimplifiedVGResponse filteredVariableGroup, string variableGroupName)
    {
        return new()
        {
            Name = variableGroupName,
            Variables = filteredVariableGroup.Variables,
            Description = filteredVariableGroup.Description,
            Type = filteredVariableGroup.Type,
        };
    }
}
