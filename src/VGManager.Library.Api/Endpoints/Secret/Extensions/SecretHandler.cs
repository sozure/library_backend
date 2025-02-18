using VGManager.Library.Api.Endpoints.Secret.Request;
using VGManager.Library.Api.Endpoints.Secret.Response;
using VGManager.Library.Services.Models.Secrets.Requests;
using VGManager.Library.Services.Models.Secrets.Results;

namespace VGManager.Library.Api.Endpoints.Secret.Extensions;

public static class SecretHandler
{
    public static SecretResponse ToResponse(this SecretResult result)
        => new()
        {
            CreatedBy = result.CreatedBy,
            KeyVault = result.KeyVault,
            SecretName = result.SecretName,
            SecretValue = result.SecretValue
        };

    public static DeletedSecretResponse ToResponse(this DeletedSecretResult result)
        => new()
        {
            KeyVault = result.KeyVault,
            SecretName = result.SecretName,
            SecretValue = result.SecretValue,
            DeletedOn = result.DeletedOn
        };

    public static SecretModel ToModel(this SecretRequest request)
        => new()
        {
            ClientId = request.ClientId,
            ClientSecret = request.ClientSecret,
            KeyVaultName = request.KeyVaultName,
            SecretFilter = request.SecretFilter,
            TenantId = request.TenantId,
            UserName = request.UserName
        };

    public static SecretBaseModel ToModel(this SecretBaseRequest request)
        => new()
        {
            UserName = request.UserName,
            TenantId = request.TenantId,
            ClientId = request.ClientId,
            ClientSecret = request.ClientSecret
        };

    public static SecretCopyModel ToModel(this SecretCopyRequest request)
        => new()
        {
            UserName = request.UserName,
            TenantId = request.TenantId,
            ClientId = request.ClientId,
            ClientSecret = request.ClientSecret,
            FromKeyVault = request.FromKeyVault,
            OverrideSecret = request.OverrideSecret,
            ToKeyVault = request.ToKeyVault
        };
}
