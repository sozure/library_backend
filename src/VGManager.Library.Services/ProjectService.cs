using System.Text.Json;
using VGManager.Adapter.Models.Kafka;
using VGManager.Adapter.Models.Models;
using VGManager.Adapter.Models.Requests;
using VGManager.Adapter.Models.Requests.VG;
using VGManager.Adapter.Models.Response;
using VGManager.Adapter.Models.StatusEnums;
using VGManager.Library.Services.Interfaces;

namespace VGManager.Library.Services;

public class ProjectService : IProjectService
{
    private readonly IAdapterCommunicator _adapterCommunicator;

    public ProjectService(
        IAdapterCommunicator adapterCommunicator
        )
    {
        _adapterCommunicator = adapterCommunicator;
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

        (var isSuccess, var response) = await _adapterCommunicator.CommunicateWithAdapterAsync(
            request,
            CommandTypes.GetProjectsRequest,
            cancellationToken
            );

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
