using AutoMapper;
using VGManager.Library.AzureAdapter.Interfaces;
using VGManager.Library.Models.Models;
using VGManager.Library.Services.Interfaces;
using VGManager.Library.Services.Models.Common;
using VGManager.Library.Services.Models.Projects;

namespace VGManager.Library.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectAdapter _projectRepository;
    private readonly IMapper _mapper;

    public ProjectService(IProjectAdapter projectRepository, IMapper mapper)
    {
        _projectRepository = projectRepository;
        _mapper = mapper;
    }

    public async Task<AdapterResponseModel<IEnumerable<ProjectResult>>> GetProjectsAsync(BaseModel projectModel, CancellationToken cancellationToken = default)
    {
        var url = $"https://dev.azure.com/{projectModel.Organization}";
        var projectsEntity = await _projectRepository.GetProjectsAsync(url, projectModel.PAT, cancellationToken);

        return new()
        {
            Status = projectsEntity.Status,
            Data = _mapper.Map<IEnumerable<ProjectResult>>(projectsEntity.Data)
        };
    }
}
