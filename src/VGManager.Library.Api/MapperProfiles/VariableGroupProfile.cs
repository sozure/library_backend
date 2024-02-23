using AutoMapper;
using VGManager.Adapter.Models.Requests.VG;
using VGManager.Library.Api.Endpoints.VariableGroup.Request;
using VGManager.Library.Api.Endpoints.VariableGroup.Response;
using VGManager.Library.Services.Models.VariableGroups.Results;

namespace VGManager.Library.Api.MapperProfiles;

public class VariableGroupProfile : Profile
{
    public VariableGroupProfile()
    {
        CreateMap<VariableUpdateRequest, VariableGroupUpdateModel>();
        CreateMap<VariableAddRequest, VariableGroupAddModel>();
        CreateMap<VariableRequest, VariableGroupModel>();

        CreateMap<VariableResult, VariableResponse>();
    }
}
