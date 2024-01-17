using AutoMapper;
using VGManager.Library.AzureAdapter.Entities;
using VGManager.Library.Services.Models.Projects;

namespace VGManager.Library.Services.MapperProfiles;
public class ProjectProfile : Profile
{
    public ProjectProfile()
    {
        CreateMap<ProjectEntity, ProjectResult>();
    }
}
