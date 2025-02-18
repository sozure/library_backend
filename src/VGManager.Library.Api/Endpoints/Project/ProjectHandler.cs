using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using VGManager.Adapter.Models.Models;
using VGManager.Library.Api.Common;
using VGManager.Library.Api.Endpoints.Project.Extensions;
using VGManager.Library.Api.Endpoints.Project.Response;
using VGManager.Library.Services.Interfaces;

namespace VGManager.Library.Api.Endpoints.Project;

public static class ProjectHandler
{
    [ExcludeFromCodeCoverage]
    public static RouteGroupBuilder MapProjectHandler(this RouteGroupBuilder builder)
    {
        var group = builder.MapGroup("/project");

        group.MapPost("/", GetAsync)
            .WithName("GetProjects");

        return builder;
    }

    public static async Task<Ok<AdapterResponseModel<IEnumerable<ProjectResponse>>>> GetAsync(
        [FromBody] BasicRequest request,
        [FromServices] IProjectService projectService,
        CancellationToken cancellationToken
    )
    {
        var projectModel = request.ToModel();
        var projects = await projectService.GetProjectsAsync(projectModel, cancellationToken);

        var result = new AdapterResponseModel<IEnumerable<ProjectResponse>>()
        {
            Status = projects.Status,
            Data = projects.Data.Select(x => x.ToResponse())
        };

        return TypedResults.Ok(result);
    }
}
