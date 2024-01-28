using AutoMapper;
using VGManager.Library.Api.Endpoints.Secret.Request;
using VGManager.Library.Api.Endpoints.Secret.Response;
using VGManager.Library.Services.Models.Secrets.Requests;
using VGManager.Library.Services.Models.Secrets.Results;

namespace VGManager.Library.Api.MapperProfiles;

public class SecretProfile : Profile
{
    public SecretProfile()
    {
        CreateMap<SecretRequest, SecretModel>();
        CreateMap<SecretBaseRequest, SecretBaseModel>();
        CreateMap<SecretCopyRequest, SecretCopyModel>();

        CreateMap<SecretResult, SecretResponse>();
        CreateMap<DeletedSecretResult, DeletedSecretResponse>();
    }
}
