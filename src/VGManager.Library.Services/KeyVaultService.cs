using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using VGManager.Library.AzureAdapter.Interfaces;
using VGManager.Library.Entities.SecretEntities;
using VGManager.Library.Models.Models;
using VGManager.Library.Models.StatusEnums;
using VGManager.Library.Repositories.Interfaces.SecretRepositories;
using VGManager.Library.Services.Interfaces;
using VGManager.Library.Services.Models.Secrets.Requests;
using VGManager.Library.Services.Models.Secrets.Results;

namespace VGManager.Library.Services;

public class KeyVaultService : IKeyVaultService
{
    private readonly IKeyVaultAdapter _keyVaultConnectionRepository;
    private readonly ISecretChangeColdRepository _secretChangeColdRepository;
    private readonly IKeyVaultCopyColdRepository _keyVaultCopyColdRepository;
    private string _keyVault = null!;
    private readonly ILogger _logger;

    public KeyVaultService(
        IKeyVaultAdapter keyVaultConnectionRepository,
        ISecretChangeColdRepository secretChangeColdRepository,
        IKeyVaultCopyColdRepository keyVaultCopyColdRepository,
        ILogger<KeyVaultService> logger
        )
    {
        _keyVaultConnectionRepository = keyVaultConnectionRepository;
        _secretChangeColdRepository = secretChangeColdRepository;
        _keyVaultCopyColdRepository = keyVaultCopyColdRepository;
        _logger = logger;
    }

    public void SetupConnectionRepository(SecretModel secretModel)
    {
        var keyVault = secretModel.KeyVaultName;
        _keyVaultConnectionRepository.Setup(keyVault, secretModel.TenantId, secretModel.ClientId, secretModel.ClientSecret);
        _keyVault = keyVault;
    }

    public async Task<(string?, IEnumerable<string>)> GetKeyVaultsAsync(
        string tenantId,
        string clientId,
        string clientSecret,
        CancellationToken cancellationToken = default
        )
    {
        return await _keyVaultConnectionRepository.GetKeyVaultsAsync(tenantId, clientId, clientSecret, cancellationToken);
    }

    public async Task<AdapterResponseModel<IEnumerable<SecretResult>>> GetSecretsAsync(string secretFilter, CancellationToken cancellationToken = default)
    {
        var secretList = new List<SecretResult>();
        var secretsEntity = await _keyVaultConnectionRepository.GetSecretsAsync(cancellationToken);
        var status = secretsEntity.Status;
        var secrets = CollectSecrets(secretsEntity);

        if (status == AdapterStatus.Success)
        {
            var filteredSecrets = Filter(secrets, secretFilter);

            foreach (var filteredSecret in filteredSecrets)
            {
                CollectSecrets(secretList, filteredSecret);
            }

            return GetResult(status, secretList);
        }
        return GetResult(status, secretList);
    }

    public async Task<AdapterStatus> CopySecretsAsync(SecretCopyModel secretCopyModel, CancellationToken cancellationToken = default)
    {
        try
        {
            _keyVaultConnectionRepository.Setup(
            secretCopyModel.FromKeyVault,
            secretCopyModel.TenantId,
            secretCopyModel.ClientId,
            secretCopyModel.ClientSecret
            );

            var fromSecrets = await _keyVaultConnectionRepository.GetAllAsync(cancellationToken);

            _keyVaultConnectionRepository.Setup(
                secretCopyModel.ToKeyVault,
                secretCopyModel.TenantId,
                secretCopyModel.ClientId,
                secretCopyModel.ClientSecret
                );

            var toSecrets = await _keyVaultConnectionRepository.GetAllAsync(cancellationToken);

            foreach (var secret in fromSecrets)
            {
                var parameters = ParametersBuilder(secret, toSecrets, secretCopyModel.OverrideSecret);
                var partialStatus = await _keyVaultConnectionRepository.AddKeyVaultSecretAsync(parameters, cancellationToken);

                if (partialStatus != AdapterStatus.Success)
                {
                    return partialStatus;
                }
            }

            var entity = new KeyVaultCopyEntity
            {
                Date = DateTime.UtcNow,
                OriginalKeyVault = secretCopyModel.FromKeyVault,
                DestinationKeyVault = secretCopyModel.ToKeyVault,
                User = secretCopyModel.UserName
            };

            await _keyVaultCopyColdRepository.AddEntityAsync(entity, cancellationToken);

            return AdapterStatus.Success;
        }
        catch (Azure.RequestFailedException ex)
        {
            _logger.LogError(ex, "Couldn't copy secrets from {fromKeyVault} to {toKeyVault}.", secretCopyModel.FromKeyVault, secretCopyModel.ToKeyVault);
            return AdapterStatus.Unauthorized;
        }
    }

    public AdapterResponseModel<IEnumerable<DeletedSecretResult>> GetDeletedSecrets(string secretFilter, CancellationToken cancellationToken = default)
    {
        var secretList = new List<DeletedSecretResult>();
        var secretsEntity = _keyVaultConnectionRepository.GetDeletedSecrets(cancellationToken);
        var status = secretsEntity.Status;

        if (status == AdapterStatus.Success)
        {
            var filteredSecrets = Filter(secretsEntity!.Data, secretFilter);

            foreach (var filteredSecret in filteredSecrets)
            {
                secretList.Add(new()
                {
                    KeyVault = _keyVault,
                    SecretName = filteredSecret.Name,
                    SecretValue = filteredSecret.Value,
                    DeletedOn = filteredSecret.DeletedOn
                });
            }
            return GetResult(status, secretList);
        }
        return GetResult(status);
    }


    public async Task<AdapterStatus> DeleteAsync(string secretFilter, string userName, CancellationToken cancellationToken = default)
    {
        var secretsResultModel = await _keyVaultConnectionRepository.GetSecretsAsync(cancellationToken);
        var status = secretsResultModel.Status;

        if (status == AdapterStatus.Success)
        {
            return await DeleteAsync(secretFilter, userName, secretsResultModel, cancellationToken);
        }

        return status;
    }

    public async Task<AdapterStatus> RecoverSecretAsync(string secretFilter, string userName, CancellationToken cancellationToken = default)
    {
        var deletedSecretsEntity = _keyVaultConnectionRepository.GetDeletedSecrets(cancellationToken);
        var status = deletedSecretsEntity.Status;

        if (status == AdapterStatus.Success)
        {
            var filteredSecrets = Filter(deletedSecretsEntity.Data, secretFilter);
            var recoverCounter = 0;
            foreach (var secret in filteredSecrets)
            {
                var recoverStatus = await _keyVaultConnectionRepository.RecoverSecretAsync(secret.Name, cancellationToken);
                if (recoverStatus == AdapterStatus.Success)
                {
                    recoverCounter++;
                }
            }

            if (recoverCounter == filteredSecrets.Count())
            {
                var entity = new SecretChangeEntity
                {
                    ChangeType = SecretChangeType.Recover,
                    Date = DateTime.UtcNow,
                    KeyVaultName = _keyVault,
                    SecretNameRegex = secretFilter,
                    User = userName

                };
                await _secretChangeColdRepository.AddEntityAsync(entity, cancellationToken);
                return AdapterStatus.Success;
            }
            return AdapterStatus.Unknown;
        }
        return status;
    }

    private IEnumerable<AdapterResponseModel<KeyVaultSecret?>> Filter(IEnumerable<AdapterResponseModel<KeyVaultSecret?>> keyVaultSecrets, string filter)
    {
        Regex regex;
        try
        {
            regex = new Regex(filter.ToLower(), RegexOptions.None, TimeSpan.FromMilliseconds(5));
        }
        catch (RegexParseException ex)
        {
            _logger.LogError(ex, "Couldn't parse and create regex. Value: {value}.", filter);
            return Enumerable.Empty<AdapterResponseModel<KeyVaultSecret?>>();
        }
        var relevantSecrets = keyVaultSecrets.Where(secret => regex.IsMatch(secret?.Data?.Name.ToLower() ?? string.Empty)).ToList();
        var result = relevantSecrets.Select(secret => new AdapterResponseModel<KeyVaultSecret?>
        {
            Status = secret.Status,
            Data = secret.Data
        });
        return result;
    }

    private IEnumerable<DeletedSecret> Filter(IEnumerable<DeletedSecret> keyVaultSecrets, string filter)
    {
        Regex regex;
        try
        {
            regex = new Regex(filter.ToLower(), RegexOptions.None, TimeSpan.FromMilliseconds(5));
        }
        catch (RegexParseException ex)
        {
            _logger.LogError(ex, "Couldn't parse and create regex. Value: {value}.", filter);
            return Enumerable.Empty<DeletedSecret>();
        }
        return keyVaultSecrets.Where(secret => regex.IsMatch(secret?.Name.ToLower() ?? string.Empty)).ToList();
    }

    private static Dictionary<string, string> ParametersBuilder(
        KeyVaultSecret keyVaultSecret,
        IEnumerable<KeyVaultSecret> toKeyVaultSecrets,
        bool overrideSecret
        )
    {
        var parameters = new Dictionary<string, string>
        {
            ["secretName"] = keyVaultSecret.Name
        };

        var toKeyVaultSecret = toKeyVaultSecrets.FirstOrDefault(kv => kv.Name.Equals(keyVaultSecret.Name));
        parameters["secretValue"] = overrideSecret || toKeyVaultSecret is null ? keyVaultSecret.Value : toKeyVaultSecret.Value;

        return parameters;
    }

    private async Task<AdapterStatus> DeleteAsync(
        string secretFilter,
        string userName,
        AdapterResponseModel<IEnumerable<AdapterResponseModel<KeyVaultSecret?>>> secretsResultModel,
        CancellationToken cancellationToken
        )
    {
        var secrets = CollectSecrets(secretsResultModel);
        var filteredSecrets = Filter(secrets, secretFilter);
        var deletionCounter1 = 0;
        var deletionCounter2 = 0;

        foreach (var secret in filteredSecrets)
        {
            var secretName = secret?.Data?.Name;
            var secretValue = secret?.Data?.Value;
            if (secretName is not null && secretValue is not null)
            {
                deletionCounter1++;
                var deletionStatus = await _keyVaultConnectionRepository.DeleteSecretAsync(secretName, cancellationToken);

                if (deletionStatus == AdapterStatus.Success)
                {
                    deletionCounter2++;
                }
            }
        }

        if (deletionCounter1 == deletionCounter2)
        {
            var entity = new SecretChangeEntity
            {
                ChangeType = SecretChangeType.Delete,
                Date = DateTime.UtcNow,
                KeyVaultName = _keyVault,
                SecretNameRegex = secretFilter,
                User = userName

            };
            await _secretChangeColdRepository.AddEntityAsync(entity, cancellationToken);
            return AdapterStatus.Success;
        }

        return AdapterStatus.Unknown;
    }

    private static IEnumerable<AdapterResponseModel<KeyVaultSecret?>> CollectSecrets(
        AdapterResponseModel<IEnumerable<AdapterResponseModel<KeyVaultSecret?>>>? secretsResultModel
        )
    {
        if (secretsResultModel is null)
        {
            return Enumerable.Empty<AdapterResponseModel<KeyVaultSecret?>>();
        }

        var relevantSecrets = secretsResultModel.Data.Where(secret => secret != null);
        var result = relevantSecrets.Select(secret => new AdapterResponseModel<KeyVaultSecret?>
        {
            Status = secret!.Status,
            Data = secret!.Data
        });
        return result;
    }

    private void CollectSecrets(List<SecretResult> secretList, AdapterResponseModel<KeyVaultSecret?>? filteredSecret)
    {
        if (filteredSecret is not null)
        {
            var secretName = filteredSecret.Data?.Name ?? string.Empty;
            var secretValue = filteredSecret.Data?.Value ?? string.Empty;

            if (!string.IsNullOrEmpty(secretName) && !string.IsNullOrEmpty(secretValue))
            {
                secretList.Add(new()
                {
                    KeyVault = _keyVault,
                    SecretName = secretName,
                    SecretValue = secretValue
                });
            }
        }
    }

    private static AdapterResponseModel<IEnumerable<DeletedSecretResult>> GetResult(AdapterStatus status)
    {
        return new()
        {
            Status = status,
            Data = Enumerable.Empty<DeletedSecretResult>()
        };
    }

    private static AdapterResponseModel<IEnumerable<DeletedSecretResult>> GetResult(AdapterStatus status, IEnumerable<DeletedSecretResult> secretList)
    {
        return new()
        {
            Status = status,
            Data = secretList
        };
    }

    private static AdapterResponseModel<IEnumerable<SecretResult>> GetResult(AdapterStatus status, IEnumerable<SecretResult> secretList)
    {
        return new()
        {
            Status = status,
            Data = secretList
        };
    }
}
