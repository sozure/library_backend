using VGManager.Adapter.Models.Models;
using VGManager.Adapter.Models.Requests;
using VGManager.Adapter.Models.Requests.VG;
using VGManager.Adapter.Models.StatusEnums;
using VGManager.Library.Api.Endpoints.VariableGroup.Request;
using VGManager.Library.Api.Endpoints.VariableGroup.Response;

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

    private async Task<AdapterResponseModel<IEnumerable<ProjectRequest>>> GetProjectsAsync(
        VariableRequest request,
        CancellationToken cancellationToken
        )
    {
        var projectModel = new BaseModel
        {
            Organization = request.Organization,
            PAT = request.PAT
        };

        var projectResponse = await projectService.GetProjectsAsync(projectModel, cancellationToken);
        return projectResponse;
    }

    private async Task<AdapterResponseModel<List<VariableResponse>>> GetResultAfterDeleteAsync(
        VariableRequest request,
        CancellationToken cancellationToken
        )
    {
        var vgServiceModel = mapper.Map<VariableGroupModel>(request);
        var status = await variableService.DeleteVariablesAsync(vgServiceModel, true, cancellationToken);
        var variableGroupResultModel = await variableService.GetVariablesAsync(vgServiceModel, cancellationToken);

        var result = new AdapterResponseModel<List<VariableResponse>>()
        {
            Status = variableGroupResultModel.Status,
            Data = mapper.Map<List<VariableResponse>>(variableGroupResultModel.Data)
        };

        if (status != AdapterStatus.Success)
        {
            result.Status = status ?? AdapterStatus.Unknown;
        }

        return result;
    }

    private async Task<AdapterResponseModel<List<VariableResponse>>> GetBaseResultAsync(VariableRequest request, CancellationToken cancellationToken)
    {
        var vgServiceModel = mapper.Map<VariableGroupModel>(request);
        var variableGroupResultsModel = await variableService.GetVariablesAsync(vgServiceModel, cancellationToken);

        var result = new AdapterResponseModel<List<VariableResponse>>()
        {
            Status = variableGroupResultsModel.Status,
            Data = mapper.Map<List<VariableResponse>>(variableGroupResultsModel.Data)
        };

        return result;
    }

    private async Task<AdapterResponseModel<IEnumerable<VariableGroupResponse>>> GetVGResultAsync(
        VariableGroupRequest request,
        CancellationToken cancellationToken
        )
    {
        var vgServiceModel = mapper.Map<VariableGroupModel>(request);
        var variableGroupResultsModel = await vgService.GetVariableGroupsAsync(
            vgServiceModel,
            request.PotentialVariableGroups?.Select(vgs => vgs.Name) ?? null!,
            request.ContainsKey,
            cancellationToken
            );

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
        var vgServiceModel = mapper.Map<VariableGroupAddModel>(request);
        var status = await variableService.AddVariablesAsync(vgServiceModel, cancellationToken);
        vgServiceModel.KeyFilter = vgServiceModel.Key;
        vgServiceModel.ValueFilter = vgServiceModel.Value;
        var variableGroupResultModel = await variableService.GetVariablesAsync(vgServiceModel, cancellationToken);

        var result = new AdapterResponseModel<List<VariableResponse>>()
        {
            Status = variableGroupResultModel.Status,
            Data = mapper.Map<List<VariableResponse>>(variableGroupResultModel.Data)
        };

        if (status != AdapterStatus.Success)
        {
            result.Status = status ?? AdapterStatus.Unknown;
        }

        return result;
    }

    private async Task<AdapterResponseModel<List<VariableResponse>>> GetUpdateResultAsync(
        VariableUpdateRequest request,
        CancellationToken cancellationToken
        )
    {
        var vgServiceModel = mapper.Map<VariableGroupUpdateModel>(request);
        var status = await variableService.UpdateVariableGroupsAsync(vgServiceModel, true, cancellationToken);

        vgServiceModel.ValueFilter = vgServiceModel.NewValue;
        var variableGroupResultModel = await variableService.GetVariablesAsync(vgServiceModel, cancellationToken);

        var result = new AdapterResponseModel<List<VariableResponse>>()
        {
            Status = variableGroupResultModel.Status,
            Data = mapper.Map<List<VariableResponse>>(variableGroupResultModel.Data)
        };

        if (status != AdapterStatus.Success)
        {
            result.Status = status ?? AdapterStatus.Unknown;
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
