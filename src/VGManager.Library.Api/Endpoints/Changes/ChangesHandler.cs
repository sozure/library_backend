using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using VGManager.Adapter.Models.Models;
using VGManager.Adapter.Models.StatusEnums;
using VGManager.Library.Api.Endpoints.Changes.Extensions;
using VGManager.Library.Api.Endpoints.Changes.Request;
using VGManager.Library.Services.Interfaces;
using VGManager.Library.Services.Models.Changes.Responses;

namespace VGManager.Library.Api.Endpoints.Changes;

public static class ChangesHandler
{
    [ExcludeFromCodeCoverage]
    public static RouteGroupBuilder MapChangesHandler(this RouteGroupBuilder builder)
    {
        var group = builder.MapGroup("/changes");

        group.MapPost("/variables", GetVariableChangesAsync)
            .WithName("GetVariableChanges");

        group.MapPost("/secrets", GetSecretChangesAsync)
            .WithName("GetSecretChanges");

        group.MapPost("/keyvaults", GetKVChangesAsync)
            .WithName("GetKeyVaultChanges");

        return builder;
    }

    public static async Task<Ok<RepositoryResponseModel<VGOperationModel>>> GetVariableChangesAsync(
        [FromBody] VGChangesRequest request,
        [FromServices] IChangeService changeService,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var result = await changeService.GetAsync(
                request.ToModel(),
                cancellationToken
            );

            return TypedResults.Ok(new RepositoryResponseModel<VGOperationModel>
            {
                Status = RepositoryStatus.Success,
                Data = result
            });
        }
        catch (Exception)
        {
            return TypedResults.Ok(new RepositoryResponseModel<VGOperationModel>
            {
                Status = RepositoryStatus.Unknown,
                Data = []
            });
        }
    }

    public static async Task<Ok<RepositoryResponseModel<SecretOperationModel>>> GetSecretChangesAsync(
        [FromBody] SecretChangesRequest request,
        [FromServices] IChangeService changeService,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var result = await changeService.GetAsync(
                request.ToModel(),
                cancellationToken
            );

            return TypedResults.Ok(new RepositoryResponseModel<SecretOperationModel>
            {
                Status = RepositoryStatus.Success,
                Data = result
            });
        }
        catch (Exception)
        {
            return TypedResults.Ok(new RepositoryResponseModel<SecretOperationModel>
            {
                Status = RepositoryStatus.Unknown,
                Data = []
            });
        }
    }

    public static async Task<Ok<RepositoryResponseModel<KVOperationModel>>> GetKVChangesAsync(
        [FromBody] KVChangesRequest request,
        [FromServices] IChangeService changeService,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var result = await changeService.GetAsync(
                request.ToModel(),
                cancellationToken
            );
            return TypedResults.Ok(new RepositoryResponseModel<KVOperationModel>
            {
                Status = RepositoryStatus.Success,
                Data = result
            });
        }
        catch (Exception)
        {
            return TypedResults.Ok(new RepositoryResponseModel<KVOperationModel>
            {
                Status = RepositoryStatus.Unknown,
                Data = []
            });
        }
    }
}
