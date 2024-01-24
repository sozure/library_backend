

using VGManager.Adapter.Models.Models;
using VGManager.Adapter.Models.StatusEnums;
using VGManager.Library.Services.Models.Secrets.Requests;
using VGManager.Library.Services.Models.Secrets.Results;

namespace VGManager.Library.Services.Interfaces;

public interface IKeyVaultService
{
    Task<(string?, IEnumerable<string>)> GetKeyVaultsAsync(
        SecretBaseModel secretModel,
        CancellationToken cancellationToken = default
        );
    Task<AdapterResponseModel<IEnumerable<SecretResult>>> GetSecretsAsync(
        SecretModel secretModel, 
        CancellationToken cancellationToken = default
        );
    Task<AdapterResponseModel<IEnumerable<DeletedSecretResult>>> GetDeletedSecretsAsync(
        SecretModel secretModel, 
        CancellationToken cancellationToken = default
        );
    Task<AdapterStatus> RecoverSecretAsync(SecretModel secretModel, CancellationToken cancellationToken = default);
    Task<AdapterStatus> DeleteAsync(SecretModel secretModel, CancellationToken cancellationToken = default);
    Task<AdapterStatus> CopySecretsAsync(SecretCopyModel secretCopyModel, CancellationToken cancellationToken = default);
}
