using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using VGManager.Adapter.Models.Models;
using VGManager.Adapter.Models.Requests;
using VGManager.Adapter.Models.Requests.VG;
using VGManager.Adapter.Models.StatusEnums;
using VGManager.Library.Api.Endpoints.VariableGroup.Extensions;
using VGManager.Library.Api.Endpoints.VariableGroup.Request;
using VGManager.Library.Api.Endpoints.VariableGroup.Response;
using VGManager.Library.Services.Interfaces;

namespace VGManager.Library.Api.Endpoints.VariableGroup;

public static class VariableGroupHandler
{
    [ExcludeFromCodeCoverage]
    public static RouteGroupBuilder MapVariableGroupHandler(this RouteGroupBuilder builder)
    {
        var group = builder.MapGroup("/variablegroup");

        group.MapPost("/", GetAsync)
            .WithName("GetVariables");

        group.MapPost("/update", UpdateAsync)
            .WithName("UpdateVariables");

        group.MapPost("/updateinline", UpdateInlineAsync)
            .WithName("UpdateVariablesInline");

        group.MapPost("/add", AddAsync)
            .WithName("AddVariables");

        group.MapPost("/addinline", AddInlineAsync)
            .WithName("AddVariablesInline");

        group.MapPost("/delete", DeleteAsync)
            .WithName("DeleteVariables");

        group.MapPost("/deleteinline", DeleteInlineAsync)
            .WithName("DeleteVariablesInline");

        group.MapPost("/variablegroups", GetVariableGroupsAsync)
            .WithName("GetVariableGroups");

        return builder;
    }

    public static async Task<Ok<AdapterResponseModel<List<VariableResponse>>>> GetAsync(
        [FromBody] VariableRequest request,
        [FromServices] IProjectService projectService,
        [FromServices] IVariableService variableService,
        CancellationToken cancellationToken
    )
    {
        var result = await GetVariableGroupResponsesAsync(request, projectService, variableService, cancellationToken);
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<AdapterResponseModel<List<VariableGroupResponse>>>> GetVariableGroupsAsync(
        [FromBody] VariableGroupRequest request,
        [FromServices] IProjectService projectService,
        [FromServices] IVariableGroupService variableGroupService,
        CancellationToken cancellationToken
    )
    {
        if (request.Project == "All")
        {
            var result = GetEmptyVariableGroupGetResponses();
            var projectResponse = await GetProjectsAsync(request, projectService, cancellationToken);

            foreach (var project in projectResponse.Data)
            {
                request.Project = project.Project.Name;
                var subResult = await GetVGResultAsync(request, variableGroupService, cancellationToken);
                result.Data.AddRange(subResult.Data);

                if (subResult.Status != AdapterStatus.Success)
                {
                    result.Status = subResult.Status;
                }
            }
            return TypedResults.Ok(result);
        }
        else
        {
            var result = await GetVGResultAsync(request, variableGroupService, cancellationToken);
            return TypedResults.Ok(result);
        }
    }

    public static async Task<Ok<AdapterResponseModel<List<VariableResponse>>>> UpdateAsync(
        [FromBody] VariableUpdateRequest request,
        [FromServices] IProjectService projectService,
        [FromServices] IVariableService variableService,
        CancellationToken cancellationToken
    )
    {
        var result = await GetVariableGroupResponsesAsync(request, projectService, variableService, cancellationToken);
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<AdapterStatus>> UpdateInlineAsync(
        [FromBody] VariableUpdateRequest request,
        [FromServices] IVariableService variableService,
        CancellationToken cancellationToken
    )
    {
        var vgServiceModel = request.ToModel();
        var status = await variableService.UpdateVariableGroupsAsync(vgServiceModel, false, cancellationToken);

        return TypedResults.Ok(status ?? AdapterStatus.Unknown);
    }

    public static async Task<Ok<AdapterResponseModel<List<VariableResponse>>>> AddAsync(
        [FromBody] VariableAddRequest request,
        [FromServices] IProjectService projectService,
        [FromServices] IVariableService variableService,
        CancellationToken cancellationToken
    )
    {
        var result = await GetVariableGroupResponsesAsync(request, projectService, variableService, cancellationToken);
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<AdapterStatus>> AddInlineAsync(
        [FromBody] VariableAddRequest request,
        [FromServices] IVariableService variableService,
        CancellationToken cancellationToken
    )
    {
        var vgServiceModel = request.ToModel();
        var status = await variableService.AddVariablesAsync(vgServiceModel, cancellationToken);
        return TypedResults.Ok(status ?? AdapterStatus.Unknown);
    }

    public static async Task<Ok<AdapterResponseModel<List<VariableResponse>>>> DeleteAsync(
        [FromBody] VariableChangeRequest request,
        [FromServices] IProjectService projectService,
        [FromServices] IVariableService variableService,
        CancellationToken cancellationToken
    )
    {
        AdapterResponseModel<List<VariableResponse>> result;
        if (request.Project == "All")
        {
            result = GetEmptyVariablesGetResponses();
            var projectResponse = await GetProjectsAsync(request, projectService, cancellationToken);

            foreach (var project in projectResponse.Data)
            {
                request.Project = project.Project.Name;
                var subResult = await GetResultAfterDeleteAsync(request, variableService, cancellationToken);
                result.Data.AddRange(subResult.Data);

                if (subResult.Status != AdapterStatus.Success)
                {
                    result.Status = subResult.Status;
                }
            }
        }
        else
        {
            result = await GetResultAfterDeleteAsync(request, variableService, cancellationToken);
        }
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<AdapterStatus>> DeleteInlineAsync(
        [FromBody] VariableChangeRequest request,
        [FromServices] IVariableService variableService,
        CancellationToken cancellationToken
    )
    {
        var vgServiceModel = request.ToModel();
        var status = await variableService.DeleteVariablesAsync(vgServiceModel, false, cancellationToken);
        return TypedResults.Ok(status ?? AdapterStatus.Unknown);
    }

    private static AdapterResponseModel<List<VariableResponse>> GetEmptyVariablesGetResponses()
     => new()
     {
         Status = AdapterStatus.Success,
         Data = []
     };

    private static AdapterResponseModel<List<VariableGroupResponse>> GetEmptyVariableGroupGetResponses()
        => new()
        {
            Status = AdapterStatus.Success,
            Data = []
        };

    private static async Task<AdapterResponseModel<List<ProjectRequest>>> GetProjectsAsync(
        VariableRequest request,
        IProjectService projectService,
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

    private static async Task<AdapterResponseModel<List<VariableResponse>>> GetResultAfterDeleteAsync(
        VariableChangeRequest request,
        IVariableService variableService,
        CancellationToken cancellationToken
    )
    {
        var vgServiceModel = request.ToModel();
        var status = await variableService.DeleteVariablesAsync(vgServiceModel, true, cancellationToken);
        var variableGroupResultModel = await variableService.GetVariablesAsync(vgServiceModel, cancellationToken);

        var result = new AdapterResponseModel<List<VariableResponse>>()
        {
            Status = variableGroupResultModel.Status,
            Data = variableGroupResultModel.Data.Select(x => x.ToResponse()).ToList()
        };

        if (status != AdapterStatus.Success)
        {
            result.Status = status ?? AdapterStatus.Unknown;
        }

        return result;
    }

    private static async Task<AdapterResponseModel<List<VariableResponse>>> GetBaseResultAsync(
        VariableRequest request,
        IVariableService variableService,
        CancellationToken cancellationToken
    )
    {
        var vgServiceModel = request.ToModel();
        var variableGroupResultsModel = await variableService.GetVariablesAsync(vgServiceModel, cancellationToken);

        var result = new AdapterResponseModel<List<VariableResponse>>()
        {
            Status = variableGroupResultsModel.Status,
            Data = variableGroupResultsModel.Data.Select(x => x.ToResponse()).ToList()
        };

        return result;
    }

    private static async Task<AdapterResponseModel<List<VariableGroupResponse>>> GetVGResultAsync(
        VariableGroupRequest request,
        IVariableGroupService variableGroupService,
        CancellationToken cancellationToken
    )
    {
        var vgServiceModel = request.ToModel();
        var variableGroupResultsModel = await variableGroupService.GetVariableGroupsAsync(
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

    private static async Task<AdapterResponseModel<List<VariableResponse>>> GetAddResultAsync(
        VariableAddRequest request,
        IVariableService variableService,
        CancellationToken cancellationToken
    )
    {
        var vgServiceModel = request.ToModel();
        var status = await variableService.AddVariablesAsync(vgServiceModel, cancellationToken);
        vgServiceModel.KeyFilter = vgServiceModel.Key;
        vgServiceModel.ValueFilter = vgServiceModel.Value;
        var variableGroupResultModel = await variableService.GetVariablesAsync(vgServiceModel, cancellationToken);

        var result = new AdapterResponseModel<List<VariableResponse>>()
        {
            Status = variableGroupResultModel.Status,
            Data = variableGroupResultModel.Data.Select(x => x.ToResponse()).ToList()
        };

        if (status != AdapterStatus.Success)
        {
            result.Status = status ?? AdapterStatus.Unknown;
        }

        return result;
    }

    private static async Task<AdapterResponseModel<List<VariableResponse>>> GetUpdateResultAsync(
        VariableUpdateRequest request,
        IVariableService variableService,
        CancellationToken cancellationToken
    )
    {
        var vgServiceModel = request.ToModel();
        var status = await variableService.UpdateVariableGroupsAsync(vgServiceModel, true, cancellationToken);

        vgServiceModel.ValueFilter = vgServiceModel.NewValue;
        var variableGroupResultModel = await variableService.GetVariablesAsync(vgServiceModel, cancellationToken);

        var result = new AdapterResponseModel<List<VariableResponse>>()
        {
            Status = variableGroupResultModel.Status,
            Data = variableGroupResultModel.Data.Select(x => x.ToResponse()).ToList()
        };

        if (status != AdapterStatus.Success)
        {
            result.Status = status ?? AdapterStatus.Unknown;
        }

        return result;
    }

    private static async Task<AdapterResponseModel<List<VariableResponse>>> GetVariableGroupResponsesAsync<T>(
        T request,
        IProjectService projectService,
        IVariableService variableService,
        CancellationToken cancellationToken
    )
    {
        AdapterResponseModel<List<VariableResponse>>? result;
        var vgRequest = request as VariableRequest ?? new VariableRequest();
        if (vgRequest.Project == "All")
        {
            result = GetEmptyVariablesGetResponses();
            var projectResponse = await GetProjectsAsync(vgRequest, projectService, cancellationToken);

            foreach (var project in projectResponse.Data)
            {
                vgRequest.Project = project.Project.Name;
                var subResult = await GetResultAsync(request, vgRequest, variableService, cancellationToken);
                result.Data.AddRange(subResult.Data);

                if (subResult.Status != AdapterStatus.Success)
                {
                    result.Status = subResult.Status;
                }
            }
        }
        else
        {
            result = await GetResultAsync(request, vgRequest, variableService, cancellationToken);
        }
        return result;
    }

    private static async Task<AdapterResponseModel<List<VariableResponse>>> GetResultAsync<T>(
        T request,
        VariableRequest vgRequest,
        IVariableService variableService,
        CancellationToken cancellationToken
        )
    {
        if (request is VariableUpdateRequest updateRequest)
        {
            return await GetUpdateResultAsync(updateRequest, variableService, cancellationToken);
        }
        else if (request is VariableAddRequest addRequest)
        {
            return await GetAddResultAsync(addRequest, variableService, cancellationToken);
        }
        else
        {
            return await GetBaseResultAsync(vgRequest, variableService, cancellationToken);
        }
    }
}
