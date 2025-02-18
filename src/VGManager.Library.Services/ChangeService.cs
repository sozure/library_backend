using VGManager.Library.Repositories.Interfaces.SecretRepositories;
using VGManager.Library.Repositories.Interfaces.VGRepositories;
using VGManager.Library.Services.Interfaces;
using VGManager.Library.Services.Models.Changes;
using VGManager.Library.Services.Models.Changes.Extensions;
using VGManager.Library.Services.Models.Changes.Requests;
using VGManager.Library.Services.Models.Changes.Responses;

namespace VGManager.Library.Services;

public class ChangeService(
    IVGAddColdRepository additionColdRepository,
    IVGUpdateColdRepository editionColdRepository,
    IVGDeleteColdRepository deletionColdRepository,
    IKeyVaultCopyColdRepository keyVaultCopyColdRepository,
    ISecretChangeColdRepository secretChangeColdRepository
    ) : IChangeService
{
    public async Task<IEnumerable<VGOperationModel>> GetAsync(
        VGRequestModel model,
        CancellationToken cancellationToken = default
        )
    {
        var result = new List<VGOperationModel>();
        var organization = model.Organization;
        var project = model.Project;
        var user = model.User;
        var from = model.From;
        var to = model.To;
        foreach (var changeType in model.ChangeTypes)
        {
            switch (changeType)
            {
                case ChangeType.Add:
                    var addEntities = user is null ?
                        await additionColdRepository.GetAsync(organization, project, from, to, cancellationToken) :
                        await additionColdRepository.GetAsync(organization, project, user, from, to, cancellationToken);
                    result.AddRange(addEntities.Select(x => x.ToModel()));
                    break;
                case ChangeType.Update:
                    var updateEntities = user is null ?
                        await editionColdRepository.GetAsync(organization, project, from, to, cancellationToken) :
                        await editionColdRepository.GetAsync(organization, project, user, from, to, cancellationToken);
                    result.AddRange(updateEntities.Select(x => x.ToModel()));
                    break;
                case ChangeType.Delete:
                    var deleteEntities = user is null ?
                        await deletionColdRepository.GetAsync(organization, project, from, to, cancellationToken) :
                        await deletionColdRepository.GetAsync(organization, project, user, from, to, cancellationToken);
                    result.AddRange(deleteEntities.Select(x => x.ToModel()));
                    break;
                default:
                    throw new InvalidOperationException($"ChangeType does not exist: {nameof(changeType)}");
            }
        }

        var sortedResult = result.OrderByDescending(entity => entity.Date);
        return sortedResult.Take(model.Limit);
    }

    public async Task<IEnumerable<SecretOperationModel>> GetAsync(
    SecretRequestModel model,
    CancellationToken cancellationToken = default
    )
    {
        var result = new List<SecretOperationModel>();
        var keyVaultName = model.KeyVaultName;
        var user = model.User;
        var from = model.From;
        var to = model.To;
        var secretEntities = user is null ?
        await secretChangeColdRepository.GetAsync(from, to, keyVaultName, cancellationToken) :
                await secretChangeColdRepository.GetAsync(from, to, user, keyVaultName, cancellationToken);
        result.AddRange(secretEntities.Select(x => x.ToModel()));
        var sortedResult = result.OrderByDescending(entity => entity.Date);
        return sortedResult.Take(model.Limit);
    }

    public async Task<IEnumerable<KVOperationModel>> GetAsync(
    KVRequestModel model,
    CancellationToken cancellationToken = default
    )
    {
        var result = new List<KVOperationModel>();
        var user = model.User;
        var from = model.From;
        var to = model.To;
        var secretEntities = user is null ?
        await keyVaultCopyColdRepository.GetAsync(from, to, cancellationToken) :
                await keyVaultCopyColdRepository.GetAsync(from, to, user, cancellationToken);
        result.AddRange(secretEntities.Select(x => x.ToModel()));
        var sortedResult = result.OrderByDescending(entity => entity.Date);
        return sortedResult.Take(model.Limit);
    }
}
