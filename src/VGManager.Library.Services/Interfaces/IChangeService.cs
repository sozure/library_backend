

using VGManager.Library.Services.Models.Changes.Requests;
using VGManager.Library.Services.Models.Changes.Responses;

namespace VGManager.Library.Services.Interfaces;

public interface IChangeService
{
    Task<IEnumerable<VGOperationModel>> GetAsync(
        VGRequestModel model,
        CancellationToken cancellationToken = default
        );

    Task<IEnumerable<SecretOperationModel>> GetAsync(
    SecretRequestModel model,
    CancellationToken cancellationToken = default
    );

    Task<IEnumerable<KVOperationModel>> GetAsync(
    KVRequestModel model,
    CancellationToken cancellationToken = default
    );
}
