using AutoMapper;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using VGManager.Adapter.Models.Models;
using VGManager.Adapter.Models.Requests.VG;
using VGManager.Library.Api.Common;
using VGManager.Library.Api.Endpoints.Project.Response;
using VGManager.Library.Services.Interfaces;

namespace VGManager.Library.Api.Endpoints.Project;

[Route("api/[controller]")]
[ApiController]
[EnableCors("_allowSpecificOrigins")]
public class ProjectController : ControllerBase
{
    private readonly IProjectService _projectService;
    private readonly IMapper _mapper;

    public ProjectController(IProjectService projectService, IMapper mapper)
    {
        _projectService = projectService;
        _mapper = mapper;
    }

    [HttpPost("Get", Name = "getprojects")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AdapterResponseModel<IEnumerable<ProjectResponse>>>> GetAsync(
        [FromBody] BasicRequest request,
        CancellationToken cancellationToken
    )
    {
        var projectModel = _mapper.Map<BaseModel>(request);
        var projects = await _projectService.GetProjectsAsync(projectModel, cancellationToken);

        var result = new AdapterResponseModel<IEnumerable<ProjectResponse>>()
        {
            Status = projects.Status,
            Data = _mapper.Map<IEnumerable<ProjectResponse>>(projects.Data)
        };

        return Ok(result);
    }
}
