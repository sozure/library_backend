using AutoMapper;
using VGManager.Adapter.Models.Requests.VG;
using VGManager.Library.Api.Endpoints.VariableGroup.Request;
using VGManager.Library.Api.Endpoints.VariableGroup.Response;
using VGManager.Library.Services.Models.VariableGroups.Results;
using AdapterExceptionModel = VGManager.Adapter.Models.Requests.VG.ExceptionModel;
using ApiExceptionModel = VGManager.Library.Api.Endpoints.VariableGroup.Request.ExceptionModel;

namespace VGManager.Library.Api.MapperProfiles;

public class VariableGroupProfile : Profile
{
    public VariableGroupProfile()
    {
        CreateMap<VariableUpdateRequest, VariableGroupUpdateModel>();
        CreateMap<VariableAddRequest, VariableGroupAddModel>();
        CreateMap<VariableRequest, VariableGroupModel>();
        CreateMap<VariableChangeRequest, VariableGroupChangeModel>();
        CreateMap<ApiExceptionModel, AdapterExceptionModel>();

        CreateMap<VariableResult, VariableResponse>();
    }
}
