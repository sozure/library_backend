using AutoMapper;
using VGManager.Library.Api.Endpoints.VariableGroup.Request;
using VGManager.Library.Api.Endpoints.VariableGroup.Response;
using VGManager.Library.Services.Models.VariableGroups.Requests;
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
