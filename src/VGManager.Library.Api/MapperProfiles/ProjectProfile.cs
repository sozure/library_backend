using AutoMapper;
using VGManager.Library.Api.Common;
using VGManager.Library.Api.Endpoints.Project.Response;
using VGManager.Library.Services.Models.Common;
using VGManager.Library.Services.Models.Projects;

namespace VGManager.Library.Api.MapperProfiles;

public class ProjectProfile : Profile
{
    public ProjectProfile()
    {
        CreateMap<ProjectResult, ProjectResponse>()
            .ForMember(x => x.Name, opt => opt.MapFrom(src => src.Project.Name));
        CreateMap<BasicRequest, BaseModel>();
    }
}
