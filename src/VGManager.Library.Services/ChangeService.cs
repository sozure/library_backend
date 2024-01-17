using AutoMapper;
using VGManager.Library.Repositories.Interfaces.SecretRepositories;
using VGManager.Library.Repositories.Interfaces.VGRepositories;
using VGManager.Library.Services.Interfaces;
using VGManager.Library.Services.Models.Changes;
using VGManager.Library.Services.Models.Changes.Requests;
using VGManager.Library.Services.Models.Changes.Responses;

namespace VGManager.Library.Services;

public class ChangeService : IChangeService
{
    private readonly IVGAddColdRepository _additionColdRepository;
    private readonly IVGUpdateColdRepository _editionColdRepository;
    private readonly IVGDeleteColdRepository _deletionColdRepository;
    private readonly IKeyVaultCopyColdRepository _keyVaultCopyColdRepository;
    private readonly ISecretChangeColdRepository _secretChangeColdRepository;
    private readonly IMapper _mapper;

    public ChangeService(
        IVGAddColdRepository additionColdRepository,
        IVGUpdateColdRepository editionColdRepository,
        IVGDeleteColdRepository deletionColdRepository,
        IKeyVaultCopyColdRepository keyVaultCopyColdRepository,
        ISecretChangeColdRepository secretChangeColdRepository,
        IMapper mapper
    )
    {
        _additionColdRepository = additionColdRepository;
        _editionColdRepository = editionColdRepository;
        _deletionColdRepository = deletionColdRepository;
        _keyVaultCopyColdRepository = keyVaultCopyColdRepository;
        _secretChangeColdRepository = secretChangeColdRepository;
        _mapper = mapper;
    }

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
                        await _additionColdRepository.GetAsync(organization, project, from, to, cancellationToken) :
                        await _additionColdRepository.GetAsync(organization, project, user, from, to, cancellationToken);
                    result.AddRange(_mapper.Map<IEnumerable<VGOperationModel>>(addEntities));
                    break;
                case ChangeType.Update:
                    var updateEntities = user is null ?
                        await _editionColdRepository.GetAsync(organization, project, from, to, cancellationToken) :
                        await _editionColdRepository.GetAsync(organization, project, user, from, to, cancellationToken);
                    result.AddRange(_mapper.Map<IEnumerable<VGOperationModel>>(updateEntities));
                    break;
                case ChangeType.Delete:
                    var deleteEntities = user is null ?
                        await _deletionColdRepository.GetAsync(organization, project, from, to, cancellationToken) :
                        await _deletionColdRepository.GetAsync(organization, project, user, from, to, cancellationToken);
                    result.AddRange(_mapper.Map<IEnumerable<VGOperationModel>>(deleteEntities));
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
        await _secretChangeColdRepository.GetAsync(from, to, keyVaultName, cancellationToken) :
                await _secretChangeColdRepository.GetAsync(from, to, user, keyVaultName, cancellationToken);
        result.AddRange(_mapper.Map<IEnumerable<SecretOperationModel>>(secretEntities));
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
        await _keyVaultCopyColdRepository.GetAsync(from, to, cancellationToken) :
                await _keyVaultCopyColdRepository.GetAsync(from, to, user, cancellationToken);
        result.AddRange(_mapper.Map<IEnumerable<KVOperationModel>>(secretEntities));
        var sortedResult = result.OrderByDescending(entity => entity.Date);
        return sortedResult.Take(model.Limit);
    }
}
