using System.Text.Json;
using VGManager.Adapter.Models.Kafka;
using VGManager.Adapter.Models.Models;
using VGManager.Adapter.Models.Requests;
using VGManager.Adapter.Models.Requests.VG;
using VGManager.Adapter.Models.Response;
using VGManager.Adapter.Models.StatusEnums;
using VGManager.Library.Services.Interfaces;

namespace VGManager.Library.Services;

public class ProjectService(
    IAdapterCommunicator adapterCommunicator
        ) : IProjectService
{
    public async Task<AdapterResponseModel<List<ProjectRequest>>> GetProjectsAsync(
        BaseModel projectModel,
        CancellationToken cancellationToken = default
        )
    {
        var request = new BaseRequest()
        {
            Organization = projectModel.Organization,
            PAT = projectModel.PAT
        };

        (var isSuccess, var response) = await adapterCommunicator.CommunicateWithAdapterAsync(
            request,
            CommandTypes.GetProjectsRequest,
            cancellationToken
            );

        if (!isSuccess)
        {
            return new AdapterResponseModel<List<ProjectRequest>>()
            {
                Data = []
            };
        }

        var result = JsonSerializer.Deserialize<BaseResponse<AdapterResponseModel<List<ProjectRequest>>>>(response)?.Data;

        return result ?? new AdapterResponseModel<List<ProjectRequest>>()
        {
            Status = AdapterStatus.Unknown,
            Data = []
        };
    }
}
