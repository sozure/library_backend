using VGManager.Adapter.Models.Models;
using VGManager.Adapter.Models.StatusEnums;
using VGManager.Library.Api.Endpoints.VariableGroup.Request;
using VGManager.Library.Api.Endpoints.VariableGroup.Response;
using VGManager.Library.Services.Models.Common;
using VGManager.Library.Services.Models.Projects;
using VGManager.Library.Services.Models.VariableGroups.Requests;

namespace VGManager.Library.Api.Endpoints.VariableGroup;

public partial class VariableGroupController
{
    private static AdapterResponseModel<List<VariableResponse>> GetEmptyVariablesGetResponses()
    {
        return new()
        {
            Status = AdapterStatus.Success,
            Data = new List<VariableResponse>()
        };

    }

    private static AdapterResponseModel<List<VariableGroupResponse>> GetEmptyVariableGroupGetResponses()
    {
        return new()
        {
            Status = AdapterStatus.Success,
            Data = new List<VariableGroupResponse>()
        };
    }

    private async Task<AdapterResponseModel<IEnumerable<ProjectResult>>> GetProjectsAsync(VariableRequest request, CancellationToken cancellationToken)
    {
        var projectModel = new BaseModel
        {
            Organization = request.Organization,
            PAT = request.PAT
        };

        var projectResponse = await _projectService.GetProjectsAsync(projectModel, cancellationToken);
        return projectResponse;
    }

    private async Task<AdapterResponseModel<List<VariableResponse>>> GetResultAfterDeleteAsync(
        VariableRequest request,
        CancellationToken cancellationToken
        )
    {
        var vgServiceModel = _mapper.Map<VariableGroupModel>(request);

        _variableService.SetupConnectionRepository(vgServiceModel);
        var status = await _variableService.DeleteVariablesAsync(vgServiceModel, true, cancellationToken);
        var variableGroupResultModel = await _variableService.GetVariablesAsync(vgServiceModel, cancellationToken);

        var result = new AdapterResponseModel<List<VariableResponse>>()
        {
            Status = variableGroupResultModel.Status,
            Data = _mapper.Map<List<VariableResponse>>(variableGroupResultModel.Data)
        };

        if (status != AdapterStatus.Success)
        {
            result.Status = status;
        }

        return result;
    }

    private async Task<AdapterResponseModel<List<VariableResponse>>> GetBaseResultAsync(VariableRequest request, CancellationToken cancellationToken)
    {
        var vgServiceModel = _mapper.Map<VariableGroupModel>(request);

        _variableService.SetupConnectionRepository(vgServiceModel);
        var variableGroupResultsModel = await _variableService.GetVariablesAsync(vgServiceModel, cancellationToken);

        var result = new AdapterResponseModel<List<VariableResponse>>()
        {
            Status = variableGroupResultsModel.Status,
            Data = _mapper.Map<List<VariableResponse>>(variableGroupResultsModel.Data)
        };

        return result;
    }

    private async Task<AdapterResponseModel<IEnumerable<VariableGroupResponse>>> GetVGResultAsync(
        VariableGroupRequest request,
        CancellationToken cancellationToken
        )
    {
        var vgServiceModel = _mapper.Map<VariableGroupModel>(request);

        _variableService.SetupConnectionRepository(vgServiceModel);
        var variableGroupResultsModel = await _vgService.GetVariableGroupsAsync(vgServiceModel, request.ContainsKey, cancellationToken);

        var result = new List<VariableGroupResponse>();

        foreach (var variableGroup in variableGroupResultsModel.Data)
        {
            result.Add(new()
            {
                Project = request.Project,
                VariableGroupName = variableGroup.Name,
                VariableGroupType = variableGroup.Type
            });
        }

        return new()
        {
            Status = variableGroupResultsModel.Status,
            Data = result
        };
    }

    private async Task<AdapterResponseModel<List<VariableResponse>>> GetAddResultAsync(VariableAddRequest request, CancellationToken cancellationToken)
    {
        var vgServiceModel = _mapper.Map<VariableGroupAddModel>(request);

        _variableService.SetupConnectionRepository(vgServiceModel);
        var status = await _variableService.AddVariablesAsync(vgServiceModel, cancellationToken);
        vgServiceModel.KeyFilter = vgServiceModel.Key;
        vgServiceModel.ValueFilter = vgServiceModel.Value;
        var variableGroupResultModel = await _variableService.GetVariablesAsync(vgServiceModel, cancellationToken);

        var result = new AdapterResponseModel<List<VariableResponse>>()
        {
            Status = variableGroupResultModel.Status,
            Data = _mapper.Map<List<VariableResponse>>(variableGroupResultModel.Data)
        };

        if (status != AdapterStatus.Success)
        {
            result.Status = status;
        }

        return result;
    }

    private async Task<AdapterResponseModel<List<VariableResponse>>> GetUpdateResultAsync(
        VariableUpdateRequest request,
        CancellationToken cancellationToken
        )
    {
        var vgServiceModel = _mapper.Map<VariableGroupUpdateModel>(request);

        _variableService.SetupConnectionRepository(vgServiceModel);
        var status = await _variableService.UpdateVariableGroupsAsync(vgServiceModel, true, cancellationToken);

        vgServiceModel.ValueFilter = vgServiceModel.NewValue;
        var variableGroupResultModel = await _variableService.GetVariablesAsync(vgServiceModel, cancellationToken);

        var result = new AdapterResponseModel<List<VariableResponse>>()
        {
            Status = variableGroupResultModel.Status,
            Data = _mapper.Map<List<VariableResponse>>(variableGroupResultModel.Data)
        };

        if (status != AdapterStatus.Success)
        {
            result.Status = status;
        }

        return result;
    }

    private async Task<AdapterResponseModel<List<VariableResponse>>> GetVariableGroupResponsesAsync<T>(
        T request,
        CancellationToken cancellationToken
        )
    {
        AdapterResponseModel<List<VariableResponse>>? result;
        var vgRequest = request as VariableRequest ?? new VariableRequest();
        if (vgRequest.Project == "All")
        {
            result = GetEmptyVariablesGetResponses();
            var projectResponse = await GetProjectsAsync(vgRequest, cancellationToken);

            foreach (var project in projectResponse.Data)
            {
                vgRequest.Project = project.Project.Name;
                var subResult = await GetResultAsync(request, vgRequest, cancellationToken);
                result.Data.AddRange(subResult.Data);

                if (subResult.Status != AdapterStatus.Success)
                {
                    result.Status = subResult.Status;
                }
            }
        }
        else
        {
            result = await GetResultAsync(request, vgRequest, cancellationToken);
        }
        return result;
    }

    private async Task<AdapterResponseModel<List<VariableResponse>>> GetResultAsync<T>(
        T request,
        VariableRequest vgRequest,
        CancellationToken cancellationToken
        )
    {
        if (request is VariableUpdateRequest updateRequest)
        {
            return await GetUpdateResultAsync(updateRequest, cancellationToken);
        }
        else if (request is VariableAddRequest addRequest)
        {
            return await GetAddResultAsync(addRequest, cancellationToken);
        }
        else
        {
            return await GetBaseResultAsync(vgRequest, cancellationToken);
        }
    }

    private static AdapterResponseModel<IEnumerable<VariableGroupResponse>> GetResult(AdapterResponseModel<List<VariableResponse>> variableResponses)
    {
        var listResult = new List<VariableGroupResponse>();
        var result = new AdapterResponseModel<IEnumerable<VariableGroupResponse>>
        {
            Status = variableResponses.Status
        };

        foreach (var variableResponse in variableResponses.Data)
        {
            if (!listResult.Exists(
                item => item.VariableGroupName == variableResponse.VariableGroupName && item.Project == variableResponse.Project
                ))
            {
                listResult.Add(new()
                {
                    VariableGroupName = variableResponse.VariableGroupName,
                    Project = variableResponse.Project
                });
            }
        }

        result.Data = listResult;
        return result;
    }
}
