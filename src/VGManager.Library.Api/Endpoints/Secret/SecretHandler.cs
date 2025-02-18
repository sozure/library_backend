using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using VGManager.Adapter.Models.Models;
using VGManager.Adapter.Models.StatusEnums;
using VGManager.Library.Api.Endpoints.Secret.Extensions;
using VGManager.Library.Api.Endpoints.Secret.Request;
using VGManager.Library.Api.Endpoints.Secret.Response;
using VGManager.Library.Services.Interfaces;

namespace VGManager.Library.Api.Endpoints.Secret;

public static class SecretHandler
{
    [ExcludeFromCodeCoverage]
    public static RouteGroupBuilder MapSecretHandler(this RouteGroupBuilder builder)
    {
        var group = builder.MapGroup("/secret");

        group.MapPost("/GetKeyVaults", GetKeyVaultsAsync)
            .WithName("GetKeyVaults");

        group.MapPost("/Get", GetAsync)
            .WithName("GetSecrets");

        group.MapPost("/GetDeleted", GetDeletedAsync)
            .WithName("GetDeletedSecrets");

        group.MapPost("/Delete", DeleteAsync)
            .WithName("DeleteSecrets");

        group.MapPost("/DeleteInline", DeleteInlineAsync)
            .WithName("DeleteSecretInline");

        group.MapPost("/Recover", RecoverAsync)
            .WithName("RecoverSecrets");

        group.MapPost("/RecoverInline", RecoverInlineAsync)
            .WithName("RecoverSecretInline");

        group.MapPost("/Copy", CopyAsync)
            .WithName("CopySecrets");

        return builder;
    }

    public static async Task<Ok<AdapterResponseModel<IEnumerable<string>, string>>> GetKeyVaultsAsync(
        [FromBody] SecretBaseRequest request,
        [FromServices] IKeyVaultService keyVaultService,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var model = request.ToModel();
            (var subscriptionId, var keyVaults) = await keyVaultService.GetKeyVaultsAsync(model, cancellationToken);
            var result = new AdapterResponseModel<IEnumerable<string>, string>
            {
                Status = AdapterStatus.Success,
                AdditionalData = subscriptionId?.Replace("/subscriptions/", string.Empty) ?? string.Empty,
                Data = keyVaults
            };
            return TypedResults.Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message == "No subscriptions found for the given credentials")
            {
                return TypedResults.Ok(new AdapterResponseModel<IEnumerable<string>, string>
                {
                    Status = AdapterStatus.NoSubscriptionsFound,
                    Data = []
                });
            }

            return TypedResults.Ok(new AdapterResponseModel<IEnumerable<string>, string>
            {
                Status = AdapterStatus.Unknown,
                Data = []
            });
        }
        catch (Exception)
        {
            return TypedResults.Ok(new AdapterResponseModel<IEnumerable<string>, string>
            {
                Status = AdapterStatus.Unknown,
                Data = []
            });
        }
    }

    public static async Task<Ok<AdapterResponseModel<IEnumerable<SecretResponse>>>> GetAsync(
        [FromBody] SecretRequest request,
        [FromServices] IKeyVaultService keyVaultService,
        CancellationToken cancellationToken
    )
    {
        var secretModel = request.ToModel();
        var matchedSecrets = await keyVaultService.GetSecretsAsync(secretModel, cancellationToken);

        var result = new AdapterResponseModel<IEnumerable<SecretResponse>>()
        {
            Status = matchedSecrets.Status,
            Data = matchedSecrets.Data.Select(x => x.ToResponse())
        };

        return TypedResults.Ok(result);
    }

    public static async Task<Ok<AdapterResponseModel<IEnumerable<DeletedSecretResponse>>>> GetDeletedAsync(
        [FromBody] SecretRequest request,
        [FromServices] IKeyVaultService keyVaultService,
        CancellationToken cancellationToken
    )
    {
        var secretModel = request.ToModel();
        var matchedDeletedSecrets = await keyVaultService.GetDeletedSecretsAsync(secretModel, cancellationToken);

        var result = new AdapterResponseModel<IEnumerable<DeletedSecretResponse>>()
        {
            Status = matchedDeletedSecrets.Status,
            Data = matchedDeletedSecrets.Data.Select(x => x.ToResponse())
        };

        return TypedResults.Ok(result);
    }

    public static async Task<Ok<AdapterResponseModel<IEnumerable<SecretResponse>>>> DeleteAsync(
        [FromBody] SecretRequest request,
        [FromServices] IKeyVaultService keyVaultService,
        CancellationToken cancellationToken
    )
    {
        var secretModel = request.ToModel();
        await keyVaultService.DeleteAsync(secretModel, cancellationToken);
        var matchedSecrets = await keyVaultService.GetSecretsAsync(secretModel, cancellationToken);
        var data = matchedSecrets.Data.Select(x => x.ToResponse());
        var result = new AdapterResponseModel<IEnumerable<SecretResponse>>()
        {
            Status = matchedSecrets.Status,
            Data = data
        };

        return TypedResults.Ok(result);
    }

    public static async Task<Ok<AdapterStatus>> DeleteInlineAsync(
        [FromBody] SecretRequest request,
        [FromServices] IKeyVaultService keyVaultService,
        CancellationToken cancellationToken
    )
    {
        var secretModel = request.ToModel();
        var status = await keyVaultService.DeleteAsync(secretModel, cancellationToken);
        return TypedResults.Ok(status);
    }

    public static async Task<Ok<AdapterResponseModel<IEnumerable<DeletedSecretResponse>>>> RecoverAsync(
        [FromBody] SecretRequest request,
        [FromServices] IKeyVaultService keyVaultService,
        CancellationToken cancellationToken
    )
    {
        var secretModel = request.ToModel();
        await keyVaultService.RecoverSecretAsync(secretModel, cancellationToken);
        var matchedSecrets = await keyVaultService.GetDeletedSecretsAsync(secretModel, cancellationToken);

        var result = new AdapterResponseModel<IEnumerable<DeletedSecretResponse>>()
        {
            Status = matchedSecrets.Status,
            Data = matchedSecrets.Data.Select(x => x.ToResponse())
        };

        return TypedResults.Ok(result);
    }

    public static async Task<Ok<AdapterStatus>> RecoverInlineAsync(
        [FromBody] SecretRequest request,
        [FromServices] IKeyVaultService keyVaultService,
        CancellationToken cancellationToken
        )
    {
        var secretModel = request.ToModel();
        var status = await keyVaultService.RecoverSecretAsync(secretModel, cancellationToken);
        return TypedResults.Ok(status);
    }

    public static async Task<Ok<AdapterStatus>> CopyAsync(
        [FromBody] SecretCopyRequest request,
        [FromServices] IKeyVaultService keyVaultService,
        CancellationToken cancellationToken
        )
    {
        var secretModel = request.ToModel();
        var status = await keyVaultService.CopySecretsAsync(secretModel, cancellationToken);
        return TypedResults.Ok(status);
    }
}
