using AutoMapper;
using VGManager.Library.Entities.SecretEntities;
using VGManager.Library.Entities.VGEntities;
using VGManager.Library.Services.Models.Changes;
using VGManager.Library.Services.Models.Changes.Responses;

namespace VGManager.Library.Services.MapperProfiles;

public class ChangesProfile : Profile
{
    public ChangesProfile()
    {
        CreateMap<VGEntity, VGOperationModel>()
            .ForMember(src => src.Type, o => o.MapFrom(x => ChangeType.None.ToString()));
        CreateMap<VGUpdateEntity, VGOperationModel>()
            .ForMember(src => src.Type, o => o.MapFrom(x => ChangeType.Update.ToString()));
        CreateMap<VGAddEntity, VGOperationModel>()
            .ForMember(src => src.Type, o => o.MapFrom(x => ChangeType.Add.ToString()));
        CreateMap<VGDeleteEntity, VGOperationModel>()
            .ForMember(src => src.Type, o => o.MapFrom(x => ChangeType.Delete.ToString()));

        CreateMap<SecretChangeEntity, SecretOperationModel>();
        CreateMap<KeyVaultCopyEntity, KVOperationModel>();
    }
}
