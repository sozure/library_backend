using AutoMapper;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using VGManager.Adapter.Models.Models;
using VGManager.Adapter.Models.Requests.VG;
using VGManager.Adapter.Models.StatusEnums;
using VGManager.Library.Api.Endpoints.VariableGroup.Request;
using VGManager.Library.Api.Endpoints.VariableGroup.Response;
using VGManager.Library.Services.Interfaces;

namespace VGManager.Library.Api.Endpoints.VariableGroup;

[Route("api/[controller]")]
[ApiController]
[EnableCors("_allowSpecificOrigins")]
public partial class VariableGroupController(
    IVariableService variableService,
    IVariableGroupService vgService,
    IProjectService projectService,
    IMapper mapper
        ) : ControllerBase
{
    [HttpPost("Get", Name = "GetVariables")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AdapterResponseModel<List<VariableResponse>>>> GetAsync(
        [FromBody] VariableRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await GetVariableGroupResponsesAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("GetVariableGroups", Name = "GetVariableGroups")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AdapterResponseModel<IEnumerable<VariableGroupResponse>>>> GetVariableGroupsAsync(
        [FromBody] VariableGroupRequest request,
        CancellationToken cancellationToken
    )
    {
        if (request.Project == "All")
        {
            var result = GetEmptyVariableGroupGetResponses();
            var projectResponse = await GetProjectsAsync(request, cancellationToken);

            foreach (var project in projectResponse.Data)
            {
                request.Project = project.Project.Name;
                var subResult = await GetVGResultAsync(request, cancellationToken);
                result.Data.AddRange(subResult.Data);

                if (subResult.Status != AdapterStatus.Success)
                {
                    result.Status = subResult.Status;
                }
            }
            return Ok(result);
        }
        else
        {
            var result = await GetVGResultAsync(request, cancellationToken);
            return Ok(result);
        }
    }

    [HttpPost("Update", Name = "UpdateVariables")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AdapterResponseModel<List<VariableResponse>>>> UpdateAsync(
        [FromBody] VariableUpdateRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await GetVariableGroupResponsesAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("UpdateInline", Name = "UpdateVariableInline")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AdapterStatus>> UpdateInlineAsync(
        [FromBody] VariableUpdateRequest request,
        CancellationToken cancellationToken
    )
    {
        var vgServiceModel = mapper.Map<VariableGroupUpdateModel>(request);
        var status = await variableService.UpdateVariableGroupsAsync(vgServiceModel, false, cancellationToken);

        return Ok(status);
    }

    [HttpPost("Add", Name = "AddVariables")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AdapterResponseModel<List<VariableResponse>>>> AddAsync(
        [FromBody] VariableAddRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await GetVariableGroupResponsesAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("AddInline", Name = "AddVariableInline")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AdapterStatus>> AddInlineAsync(
        [FromBody] VariableAddRequest request,
        CancellationToken cancellationToken
    )
    {
        var vgServiceModel = mapper.Map<VariableGroupAddModel>(request);
        var status = await variableService.AddVariablesAsync(vgServiceModel, cancellationToken);
        return Ok(status);
    }

    [HttpPost("Delete", Name = "DeleteVariables")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AdapterResponseModel<List<VariableResponse>>>> DeleteAsync(
        [FromBody] VariableChangeRequest request,
        CancellationToken cancellationToken
    )
    {
        AdapterResponseModel<List<VariableResponse>> result;
        if (request.Project == "All")
        {
            result = GetEmptyVariablesGetResponses();
            var projectResponse = await GetProjectsAsync(request, cancellationToken);

            foreach (var project in projectResponse.Data)
            {
                request.Project = project.Project.Name;
                var subResult = await GetResultAfterDeleteAsync(request, cancellationToken);
                result.Data.AddRange(subResult.Data);

                if (subResult.Status != AdapterStatus.Success)
                {
                    result.Status = subResult.Status;
                }
            }
        }
        else
        {
            result = await GetResultAfterDeleteAsync(request, cancellationToken);
        }
        return Ok(result);
    }

    [HttpPost("DeleteInline", Name = "DeleteVariableInline")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AdapterStatus>> DeleteInlineAsync(
        [FromBody] VariableChangeRequest request,
        CancellationToken cancellationToken
    )
    {
        var vgServiceModel = mapper.Map<VariableGroupChangeModel>(request);
        var status = await variableService.DeleteVariablesAsync(vgServiceModel, false, cancellationToken);
        return Ok(status);
    }
}
