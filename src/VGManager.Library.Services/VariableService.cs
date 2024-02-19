using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using VGManager.Adapter.Models.Kafka;
using VGManager.Adapter.Models.Models;
using VGManager.Adapter.Models.Requests.VG;
using VGManager.Adapter.Models.Response;
using VGManager.Adapter.Models.StatusEnums;
using VGManager.Library.Entities.VGEntities;
using VGManager.Library.Repositories.Interfaces.VGRepositories;
using VGManager.Library.Services.Interfaces;
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

    public async Task<AdapterStatus?> UpdateVariableGroupsAsync(
        VariableGroupUpdateModel variableGroupUpdateModel,
        bool filterAsRegex,
        CancellationToken cancellationToken = default
        )
    {
        variableGroupUpdateModel.ContainsSecrets = false;
        variableGroupUpdateModel.FilterAsRegex = filterAsRegex;
        (var isSuccess, var response) = await _adapterCommunicator.CommunicateWithAdapterAsync(
            variableGroupUpdateModel,
            CommandTypes.UpdateVGRequest,
            cancellationToken
            );

        if (!isSuccess)
        {
            return AdapterStatus.Unknown;
        }

        var adapterResult = JsonSerializer.Deserialize<BaseResponse<AdapterStatus>>(response)?.Data;

        if (adapterResult is null)
        {
            return AdapterStatus.Unknown;
        }

        if (adapterResult == AdapterStatus.Success)
        {
            var variableGroupFilter = variableGroupUpdateModel.VariableGroupFilter;
            var keyFilter = variableGroupUpdateModel.KeyFilter;

            var org = variableGroupUpdateModel.Organization;

            var entity = new VGUpdateEntity
            {
                VariableGroupFilter = variableGroupFilter,
                Key = keyFilter,
                Project = variableGroupUpdateModel.Project,
                Organization = org,
                User = variableGroupUpdateModel.UserName,
                Date = DateTime.UtcNow,
                NewValue = variableGroupUpdateModel.NewValue
            };

            if (_organizationSettings.Organizations.Contains(org))
            {
                await _editionColdRepository.AddEntityAsync(entity, cancellationToken);
            }

            return adapterResult;
        }
        return adapterResult;
    }

    public async Task<AdapterStatus?> AddVariablesAsync(
        VariableGroupAddModel variableGroupAddModel,
        CancellationToken cancellationToken = default
        )
    {
        variableGroupAddModel.ContainsSecrets = false;
        variableGroupAddModel.FilterAsRegex = false;
        (var isSuccess, var response) = await _adapterCommunicator.CommunicateWithAdapterAsync(
            variableGroupAddModel,
            CommandTypes.AddVGRequest,
            cancellationToken
            );

        if (!isSuccess)
        {
            return AdapterStatus.Unknown;
        }

        var adapterResult = JsonSerializer.Deserialize<BaseResponse<AdapterStatus>>(response)?.Data;

        if (adapterResult is null)
        {
            return AdapterStatus.Unknown;
        }

        if (adapterResult == AdapterStatus.Success)
        {
            var org = variableGroupAddModel.Organization;
            var entity = new VGAddEntity
            {
                VariableGroupFilter = variableGroupAddModel.VariableGroupFilter,
                Key = variableGroupAddModel.Key,
                Value = variableGroupAddModel.Value,
                Project = variableGroupAddModel.Project,
                Organization = org,
                User = variableGroupAddModel.UserName,
                Date = DateTime.UtcNow
            };

            if (_organizationSettings.Organizations.Contains(org))
            {
                await _additionColdRepository.AddEntityAsync(entity, cancellationToken);
            }
        }
        return adapterResult;
    }

    public async Task<AdapterStatus?> DeleteVariablesAsync(
        VariableGroupModel variableGroupModel,
        bool filterAsRegex,
        CancellationToken cancellationToken = default
        )
    {
        variableGroupModel.ContainsSecrets = false;
        variableGroupModel.FilterAsRegex = filterAsRegex;
        (var isSuccess, var response) = await _adapterCommunicator.CommunicateWithAdapterAsync(
            variableGroupModel,
            CommandTypes.DeleteVGRequest,
            cancellationToken
            );

        if (!isSuccess)
        {
            return AdapterStatus.Unknown;
        }

        var adapterResult = JsonSerializer.Deserialize<BaseResponse<AdapterStatus>>(response)?.Data;

        if (adapterResult is null)
        {
            return AdapterStatus.Unknown;
        }

        if (adapterResult == AdapterStatus.Success)
        {
            var variableGroupFilter = variableGroupModel.VariableGroupFilter;
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

        return adapterResult;
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
}
