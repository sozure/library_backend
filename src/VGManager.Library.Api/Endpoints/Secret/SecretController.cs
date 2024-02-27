using AutoMapper;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using VGManager.Adapter.Models.Models;
using VGManager.Adapter.Models.StatusEnums;
using VGManager.Library.Api.Endpoints.Secret.Request;
using VGManager.Library.Api.Endpoints.Secret.Response;
using VGManager.Library.Services.Interfaces;
using VGManager.Library.Services.Models.Secrets.Requests;

namespace VGManager.Library.Api.Endpoints.Secret;

[Route("api/[controller]")]
[ApiController]
[EnableCors("_allowSpecificOrigins")]
public class SecretController(IKeyVaultService keyVaultService, IMapper mapper) : ControllerBase
{
    [HttpPost("GetKeyVaults", Name = "GetKeyVaults")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AdapterResponseModel<IEnumerable<string>, string>>> GetKeyVaults(
        [FromBody] SecretBaseRequest request,
        CancellationToken cancellationToken
        )
    {
        try
        {
            var model = mapper.Map<SecretBaseModel>(request);
            (var subscriptionId, var keyVaults) = await keyVaultService.GetKeyVaultsAsync(model, cancellationToken);
            var result = new AdapterResponseModel<IEnumerable<string>, string>
            {
                Status = AdapterStatus.Success,
                AdditionalData = subscriptionId?.Replace("/subscriptions/", string.Empty) ?? string.Empty,
                Data = keyVaults
            };
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message == "No subscriptions found for the given credentials")
            {
                return Ok(new AdapterResponseModel<IEnumerable<string>, string>
                {
                    Status = AdapterStatus.NoSubscriptionsFound,
                    Data = Enumerable.Empty<string>()
                });
            }

            return Ok(new AdapterResponseModel<IEnumerable<string>, string>
            {
                Status = AdapterStatus.Unknown,
                Data = Enumerable.Empty<string>()
            });
        }
        catch (Exception)
        {
            return Ok(new AdapterResponseModel<IEnumerable<string>, string>
            {
                Status = AdapterStatus.Unknown,
                Data = Enumerable.Empty<string>()
            });
        }
    }

    [HttpPost("Get", Name = "GetSecrets")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AdapterResponseModel<IEnumerable<SecretResponse>>>> GetAsync(
        [FromBody] SecretRequest request,
        CancellationToken cancellationToken
        )
    {
        var secretModel = mapper.Map<SecretModel>(request);
        var matchedSecrets = await keyVaultService.GetSecretsAsync(secretModel, cancellationToken);

        var result = new AdapterResponseModel<IEnumerable<SecretResponse>>()
        {
            Status = matchedSecrets.Status,
            Data = mapper.Map<IEnumerable<SecretResponse>>(matchedSecrets.Data)
        };

        return Ok(result);
    }

    [HttpPost("GetDeleted", Name = "GetDeletedSecrets")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AdapterResponseModel<IEnumerable<DeletedSecretResponse>>>> GetDeleted(
        [FromBody] SecretRequest request,
        CancellationToken cancellationToken
        )
    {
        var secretModel = mapper.Map<SecretModel>(request);
        var matchedDeletedSecrets = await keyVaultService.GetDeletedSecretsAsync(secretModel, cancellationToken);

        var result = new AdapterResponseModel<IEnumerable<DeletedSecretResponse>>()
        {
            Status = matchedDeletedSecrets.Status,
            Data = mapper.Map<IEnumerable<DeletedSecretResponse>>(matchedDeletedSecrets.Data)
        };

        return Ok(result);
    }

    [HttpPost("Delete", Name = "DeleteSecrets")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AdapterResponseModel<IEnumerable<SecretResponse>>>> DeleteAsync(
        [FromBody] SecretRequest request,
        CancellationToken cancellationToken
        )
    {
        var secretModel = mapper.Map<SecretModel>(request);
        await keyVaultService.DeleteAsync(secretModel, cancellationToken);
        var matchedSecrets = await keyVaultService.GetSecretsAsync(secretModel, cancellationToken);

        var result = new AdapterResponseModel<IEnumerable<SecretResponse>>()
        {
            Status = matchedSecrets.Status,
            Data = mapper.Map<IEnumerable<SecretResponse>>(matchedSecrets.Data)
        };

        return Ok(result);
    }

    [HttpPost("DeleteInline", Name = "DeleteSecretInline")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AdapterStatus>> DeleteInlineAsync(
        [FromBody] SecretRequest request,
        CancellationToken cancellationToken
        )
    {
        var secretModel = mapper.Map<SecretModel>(request);
        var status = await keyVaultService.DeleteAsync(secretModel, cancellationToken);
        return Ok(status);
    }

    [HttpPost("Recover", Name = "RecoverSecrets")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AdapterResponseModel<IEnumerable<DeletedSecretResponse>>>> RecoverAsync(
        [FromBody] SecretRequest request,
        CancellationToken cancellationToken
        )
    {
        var secretModel = mapper.Map<SecretModel>(request);
        await keyVaultService.RecoverSecretAsync(secretModel, cancellationToken);
        var matchedSecrets = await keyVaultService.GetDeletedSecretsAsync(secretModel, cancellationToken);

        var result = new AdapterResponseModel<IEnumerable<DeletedSecretResponse>>()
        {
            Status = matchedSecrets.Status,
            Data = mapper.Map<IEnumerable<DeletedSecretResponse>>(matchedSecrets.Data)
        };

        return Ok(result);
    }

    [HttpPost("RecoverInline", Name = "RecoverSecretInline")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AdapterStatus>> RecoverInlineAsync(
        [FromBody] SecretRequest request,
        CancellationToken cancellationToken
        )
    {
        var secretModel = mapper.Map<SecretModel>(request);
        var status = await keyVaultService.RecoverSecretAsync(secretModel, cancellationToken);
        return Ok(status);
    }

    [HttpPost("Copy", Name = "CopySecrets")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AdapterStatus>> CopyAsync(
        [FromBody] SecretCopyRequest request,
        CancellationToken cancellationToken
        )
    {
        var secretModel = mapper.Map<SecretCopyModel>(request);
        var status = await keyVaultService.CopySecretsAsync(secretModel, cancellationToken);
        return Ok(status);
    }
}
