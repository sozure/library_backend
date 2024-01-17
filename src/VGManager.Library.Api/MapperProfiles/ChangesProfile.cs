using AutoMapper;
using VGManager.Library.Api.Endpoints.Changes.Request;
using VGManager.Library.Services.Models.Changes.Requests;

namespace VGManager.Library.Api.MapperProfiles;

public class ChangesProfile : Profile
{
    public ChangesProfile()
    {
        CreateMap<VGChangesRequest, VGRequestModel>();
        CreateMap<SecretChangesRequest, SecretRequestModel>();
        CreateMap<KVChangesRequest, KVRequestModel>();
    }
}
