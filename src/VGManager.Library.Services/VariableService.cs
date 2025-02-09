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
using VGManager.Library.Services.Models.VariableGroups.Results;
using VGManager.Library.Services.Settings;

namespace VGManager.Library.Services;

public partial class VariableService(
    IAdapterCommunicator adapterCommunicator,
    IVGAddColdRepository additionColdRepository,
    IVGDeleteColdRepository deletedColdRepository,
    IVGUpdateColdRepository editionColdRepository,
    IVariableFilterService variableFilterService,
    IOptions<OrganizationSettings> organizationSettings,
    ILogger<VariableService> logger
        ) : IVariableService
{
    private readonly OrganizationSettings _organizationSettings = organizationSettings.Value;
    private readonly string SecretVGType = "AzureKeyVault";

    public async Task<AdapterStatus?> UpdateVariableGroupsAsync(
        VariableGroupUpdateModel variableGroupUpdateModel,
        bool filterAsRegex,
        CancellationToken cancellationToken = default
        )
    {
        variableGroupUpdateModel.ContainsSecrets = false;
        variableGroupUpdateModel.FilterAsRegex = filterAsRegex;
        (var isSuccess, var response) = await adapterCommunicator.CommunicateWithAdapterAsync(
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
                await editionColdRepository.AddEntityAsync(entity, cancellationToken);
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
        (var isSuccess, var response) = await adapterCommunicator.CommunicateWithAdapterAsync(
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
                await additionColdRepository.AddEntityAsync(entity, cancellationToken);
            }
        }
        return adapterResult;
    }

    public async Task<AdapterStatus?> DeleteVariablesAsync(
        VariableGroupChangeModel variableGroupModel,
        bool filterAsRegex,
        CancellationToken cancellationToken = default
        )
    {
        variableGroupModel.ContainsSecrets = false;
        variableGroupModel.FilterAsRegex = filterAsRegex;
        (var isSuccess, var response) = await adapterCommunicator.CommunicateWithAdapterAsync(
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
                await deletedColdRepository.AddEntityAsync(entity, cancellationToken);
            }
        }

        return adapterResult;
    }

    public async Task<AdapterResponseModel<IEnumerable<VariableResult>>> GetVariablesAsync(
        VariableGroupModel variableGroupModel,
        CancellationToken cancellationToken = default
        )
    {
        var vgEntity = await GetAllAsync(variableGroupModel, true, cancellationToken);
        var status = vgEntity.Status;

        if (status == AdapterStatus.Success)
        {
            return GetVariablesAsync(variableGroupModel, vgEntity, status);
        }
        else
        {
            return new()
            {
                Status = status,
                Data = [],
            };
        }
    }

    private async Task<AdapterResponseModel<IEnumerable<SimplifiedVGResponse<string>>>> GetAllAsync(
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

        (var isSuccess, var response) = await adapterCommunicator.CommunicateWithAdapterAsync(
            request,
            CommandTypes.GetAllVGRequest,
            cancellationToken
            );

        if (!isSuccess)
        {
            return new() { Data = [], Status = AdapterStatus.MessageSizeTooLarge };
        }

        var adapterResult = JsonSerializer
            .Deserialize<BaseResponse<AdapterResponseModel<IEnumerable<SimplifiedVGResponse<string>>>>>(response)?.Data;

        if (adapterResult is null)
        {
            return new() { Data = [] };
        }

        return adapterResult;
    }
}
