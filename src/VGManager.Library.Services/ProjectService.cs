using System.Text.Json;
using VGManager.Adapter.Azure.Services.Requests;
using VGManager.Adapter.Client.Interfaces;
using VGManager.Adapter.Models.Kafka;
using VGManager.Adapter.Models.Models;
using VGManager.Adapter.Models.Requests;
using VGManager.Adapter.Models.Response;
using VGManager.Adapter.Models.StatusEnums;
using VGManager.Library.Services.Interfaces;
using VGManager.Library.Services.Models.Common;
using VGManager.Library.Services.Models.Projects;

namespace VGManager.Library.Services;

public class ProjectService : IProjectService
{
    private readonly IVGManagerAdapterClientService _clientService;

    public ProjectService(
        IVGManagerAdapterClientService clientService
        )
    {
        _clientService = clientService;
    }

    public async Task<AdapterResponseModel<IEnumerable<ProjectRequest>>> GetProjectsAsync(
        BaseModel projectModel, 
        CancellationToken cancellationToken = default
        )
    {
        var request = new BaseRequest()
        {
            Organization = projectModel.Organization,
            PAT = projectModel.PAT
        };

        (bool isSuccess, string response) = await _clientService.SendAndReceiveMessageAsync(
            CommandTypes.GetProjectsRequest,
            JsonSerializer.Serialize(request),
            cancellationToken);

        if (!isSuccess)
        {
            return new AdapterResponseModel<IEnumerable<ProjectRequest>>()
            {
                Data = Enumerable.Empty<ProjectRequest>()
            };
        }

        var result = JsonSerializer.Deserialize<BaseResponse<AdapterResponseModel<IEnumerable<ProjectRequest>>>>(response)?.Data;

        return result ?? new AdapterResponseModel<IEnumerable<ProjectRequest>>()
        {
            Status = AdapterStatus.Unknown,
            Data = Enumerable.Empty<ProjectRequest>()
        };
    }
}
