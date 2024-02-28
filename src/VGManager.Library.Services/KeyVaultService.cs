using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.RegularExpressions;
using VGManager.Adapter.Models.Kafka;
using VGManager.Adapter.Models.Models;
using VGManager.Adapter.Models.Requests;
using VGManager.Adapter.Models.Response;
using VGManager.Adapter.Models.StatusEnums;
using VGManager.Library.Entities.SecretEntities;
using VGManager.Library.Repositories.Interfaces.SecretRepositories;
using VGManager.Library.Services.Interfaces;
using VGManager.Library.Services.Models.Secrets.Requests;
using VGManager.Library.Services.Models.Secrets.Results;

namespace VGManager.Library.Services;

public class KeyVaultService(
    IAdapterCommunicator adapterCommunicator,
    ISecretChangeColdRepository secretChangeColdRepository,
    IKeyVaultCopyColdRepository keyVaultCopyColdRepository,
    ILogger<KeyVaultService> logger
        ) : IKeyVaultService
{
    public async Task<(string?, IEnumerable<string>)> GetKeyVaultsAsync(
        SecretBaseModel secretModel,
        CancellationToken cancellationToken = default
        )
    {
        var request = GetBaseSecretRequest(secretModel);
        (var isSuccess, var response) = await adapterCommunicator.CommunicateWithAdapterAsync(request, CommandTypes.GetKeyVaultsRequest, cancellationToken);

        if (!isSuccess)
        {
            return (string.Empty, Enumerable.Empty<string>());
        }

        var result = JsonSerializer.Deserialize<BaseResponse<Dictionary<string, object>>>(response)?.Data;

        if (result is null)
        {
            return (string.Empty, Enumerable.Empty<string>());
        }

        int.TryParse(result["Status"].ToString(), out int i);
        var status = (AdapterStatus)i;

        if (status != AdapterStatus.Success)
        {
            return (string.Empty, Enumerable.Empty<string>());
        }

        var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(result["Data"].ToString() ?? "[]");
        var subscription = dict?["subscription"].ToString() ?? string.Empty;
        var keyVaults = JsonSerializer.Deserialize<List<string>>(dict?["keyVaults"].ToString() ?? "[]") ?? [];

        return new(subscription, keyVaults);
    }

    public async Task<AdapterResponseModel<IEnumerable<SecretResult>>> GetSecretsAsync(
        SecretModel secretModel,
        CancellationToken cancellationToken = default
        )
    {
        var secretList = new List<SecretResult>();
        var request = new SecretRequest<string>()
        {
            AdditionalData = secretModel.SecretFilter,
            ClientId = secretModel.ClientId,
            ClientSecret = secretModel.ClientSecret,
            KeyVaultName = secretModel.KeyVaultName,
            TenantId = secretModel.TenantId
        };

        (var isSuccess, var response) = await adapterCommunicator.CommunicateWithAdapterAsync(
            request,
            CommandTypes.GetSecretsRequest,
            cancellationToken
            );

        if (!isSuccess)
        {
            return new AdapterResponseModel<IEnumerable<SecretResult>>() { Data = Enumerable.Empty<SecretResult>() };
        }

        var result = JsonSerializer
            .Deserialize<BaseResponse<AdapterResponseModel<IEnumerable<AdapterResponseModel<SimplifiedSecretResponse?>>>>>(response)?.Data;

        if (result is null)
        {
            return new AdapterResponseModel<IEnumerable<SecretResult>>() { Data = Enumerable.Empty<SecretResult>() };
        }

        if (result.Status != AdapterStatus.Success)
        {
            return new AdapterResponseModel<IEnumerable<SecretResult>>() { Data = Enumerable.Empty<SecretResult>() };
        }

        var secrets = CollectSecrets(result);
        var filteredSecrets = Filter(secrets, secretModel.SecretFilter);

        foreach (var filteredSecret in filteredSecrets)
        {
            CollectSecrets(secretModel.KeyVaultName, secretList, filteredSecret);
        }

        return GetResult(result.Status, secretList);
    }

    public async Task<AdapterStatus> CopySecretsAsync(SecretCopyModel secretCopyModel, CancellationToken cancellationToken = default)
    {
        try
        {
            var fromSecrets = await GetSecretsAsync(secretCopyModel, true, cancellationToken);
            var toSecrets = await GetSecretsAsync(secretCopyModel, false, cancellationToken);

            foreach (var secret in fromSecrets)
            {
                var parameters = ParametersBuilder(secret, toSecrets, secretCopyModel.OverrideSecret);

                var request = new SecretRequest<Dictionary<string, string>>()
                {
                    ClientId = secretCopyModel.ClientId,
                    ClientSecret = secretCopyModel.ClientSecret,
                    TenantId = secretCopyModel.TenantId,
                    KeyVaultName = secretCopyModel.ToKeyVault,
                    AdditionalData = parameters
                };

                (var isSuccess, var response) = await adapterCommunicator.CommunicateWithAdapterAsync(
                    request,
                    CommandTypes.AddKeyVaultSecretRequest,
                    cancellationToken
                    );

                var result = JsonSerializer
                    .Deserialize<BaseResponse<AdapterStatus>>(response)?.Data;

                if (!isSuccess || result != AdapterStatus.Success)
                {
                    return result ?? AdapterStatus.Unknown;
                }
            }

            var entity = new KeyVaultCopyEntity
            {
                Date = DateTime.UtcNow,
                OriginalKeyVault = secretCopyModel.FromKeyVault,
                DestinationKeyVault = secretCopyModel.ToKeyVault,
                User = secretCopyModel.UserName
            };

            await keyVaultCopyColdRepository.AddEntityAsync(entity, cancellationToken);

            return AdapterStatus.Success;
        }
        catch (Azure.RequestFailedException ex)
        {
            logger.LogError(ex, "Couldn't copy secrets from {fromKeyVault} to {toKeyVault}.", secretCopyModel.FromKeyVault, secretCopyModel.ToKeyVault);
            return AdapterStatus.Unauthorized;
        }
    }

    public async Task<AdapterResponseModel<IEnumerable<DeletedSecretResult>>> GetDeletedSecretsAsync(
        SecretModel secretModel,
        CancellationToken cancellationToken = default
        )
    {
        var secretList = new List<DeletedSecretResult>();
        var request = GetBaseSecretRequest(secretModel);
        (var isSuccess, var response) = await adapterCommunicator.CommunicateWithAdapterAsync(
            request,
            CommandTypes.GetDeletedSecretsRequest,
            cancellationToken
            );

        if (!isSuccess)
        {
            return new AdapterResponseModel<IEnumerable<DeletedSecretResult>>() { Data = Enumerable.Empty<DeletedSecretResult>() };
        }

        var result = JsonSerializer.Deserialize<BaseResponse<AdapterResponseModel<IEnumerable<Dictionary<string, object>>>>>(response)?.Data;

        if (result is null)
        {
            return new AdapterResponseModel<IEnumerable<DeletedSecretResult>>() { Data = Enumerable.Empty<DeletedSecretResult>() };
        }

        var status = result.Status;

        if (status != AdapterStatus.Success)
        {
            return new AdapterResponseModel<IEnumerable<DeletedSecretResult>>() { Data = Enumerable.Empty<DeletedSecretResult>() };
        }

        var filteredSecrets = Filter(result.Data, secretModel.SecretFilter);

        foreach (var filteredSecret in filteredSecrets)
        {
            secretList.Add(new()
            {
                KeyVault = secretModel.KeyVaultName,
                SecretName = filteredSecret["Name"]?.ToString() ?? string.Empty,
                DeletedOn = DateTimeOffset.Parse(filteredSecret["DeletedOn"]?.ToString() ?? string.Empty).UtcDateTime
            });
        }
        return GetResult(status, secretList);
    }

    public async Task<AdapterStatus> DeleteAsync(
        SecretModel secretModel,
        CancellationToken cancellationToken = default
        )
    {
        var request = new SecretRequest<string>()
        {
            AdditionalData = secretModel.SecretFilter,
            ClientId = secretModel.ClientId,
            ClientSecret = secretModel.ClientSecret,
            KeyVaultName = secretModel.KeyVaultName,
            TenantId = secretModel.TenantId
        };

        (var isSuccess, var response) = await adapterCommunicator.CommunicateWithAdapterAsync(
            request,
            CommandTypes.GetSecretsRequest,
            cancellationToken
            );

        if (!isSuccess)
        {
            return AdapterStatus.Unknown;
        }

        var result = JsonSerializer
            .Deserialize<BaseResponse<AdapterResponseModel<IEnumerable<AdapterResponseModel<SimplifiedSecretResponse?>>>>>(response)?.Data;

        if (result is null)
        {
            return AdapterStatus.Unknown;
        }

        var status = result.Status;

        if (status != AdapterStatus.Success)
        {
            return status;
        }

        return await DeleteAsync(secretModel, result, cancellationToken);
    }

    public async Task<AdapterStatus> RecoverSecretAsync(
        SecretModel secretModel,
        CancellationToken cancellationToken = default
        )
    {
        var request = GetBaseSecretRequest(secretModel);
        (var isSuccess, var response) = await adapterCommunicator.CommunicateWithAdapterAsync(
            request,
            CommandTypes.GetDeletedSecretsRequest,
            cancellationToken
            );

        if (!isSuccess)
        {
            return AdapterStatus.Unknown;
        }

        var result = JsonSerializer.Deserialize<BaseResponse<AdapterResponseModel<IEnumerable<Dictionary<string, object>>>>>(response)?.Data;

        if (result is null)
        {
            return AdapterStatus.Unknown;
        }

        var status = result.Status;

        if (status != AdapterStatus.Success)
        {
            return status;
        }

        (var filteredSecretsCounter, var recoverCounter) = await GetRecoversResponseAsync(
            secretModel,
            result.Data,
            cancellationToken
            );

        if (recoverCounter == filteredSecretsCounter)
        {
            var entity = new SecretChangeEntity
            {
                ChangeType = SecretChangeType.Recover,
                Date = DateTime.UtcNow,
                KeyVaultName = secretModel.KeyVaultName,
                SecretNameRegex = secretModel.SecretFilter,
                User = secretModel.UserName
            };

            await secretChangeColdRepository.AddEntityAsync(entity, cancellationToken);
            return AdapterStatus.Success;
        }
        return AdapterStatus.Unknown;
    }

    private async Task<IEnumerable<KeyVaultSecret>> GetSecretsAsync(
        SecretCopyModel secretCopyModel,
        bool from,
        CancellationToken cancellationToken = default
        )
    {
        var request = GetBaseSecretRequest(secretCopyModel, from);
        (var isSuccess, var response) = await adapterCommunicator.CommunicateWithAdapterAsync(
            request,
            CommandTypes.GetAllSecretsRequest,
            cancellationToken
            );

        if (!isSuccess)
        {
            return Enumerable.Empty<KeyVaultSecret>();
        }

        var result = JsonSerializer.Deserialize<BaseResponse<AdapterResponseModel<IEnumerable<KeyVaultSecret>>>>(response)?.Data;

        if (result is null)
        {
            return Enumerable.Empty<KeyVaultSecret>();
        }

        var status = result.Status;

        if (status != AdapterStatus.Success)
        {
            return Enumerable.Empty<KeyVaultSecret>();
        }

        return result.Data;
    }

    private async Task<(int, int)> GetRecoversResponseAsync(
        SecretModel secretModel,
        IEnumerable<Dictionary<string, object>> secrets,
        CancellationToken cancellationToken
        )
    {
        var filteredSecrets = Filter(secrets, secretModel.SecretFilter);
        var recoverCounter = 0;
        foreach (var secret in filteredSecrets)
        {
            var recoverReq = new SecretRequest<string>()
            {
                AdditionalData = secret["Name"].ToString() ?? string.Empty,
                ClientId = secretModel.ClientId,
                ClientSecret = secretModel.ClientSecret,
                KeyVaultName = secretModel.KeyVaultName,
                TenantId = secretModel.TenantId
            };
            (var isSuccess, var response) = await adapterCommunicator.CommunicateWithAdapterAsync(
                recoverReq,
                CommandTypes.RecoverSecretRequest,
                cancellationToken
                );

            if (!isSuccess)
            {
                return (-1, 0);
            }

            var recoverStatus = JsonSerializer.Deserialize<BaseResponse<AdapterStatus>>(response)?.Data;

            if (recoverStatus != AdapterStatus.Success)
            {
                return (-1, 0);
            }

            if (recoverStatus == AdapterStatus.Success)
            {
                recoverCounter++;
            }
        }
        return (filteredSecrets.Count(), recoverCounter);
    }

    private IEnumerable<AdapterResponseModel<SimplifiedSecretResponse?>> Filter(
        IEnumerable<AdapterResponseModel<SimplifiedSecretResponse?>> keyVaultSecrets,
        string filter
        )
    {
        Regex regex;
        try
        {
            regex = new Regex(filter.ToLower(), RegexOptions.None, TimeSpan.FromMilliseconds(5));
        }
        catch (RegexParseException ex)
        {
            logger.LogError(ex, "Couldn't parse and create regex. Value: {value}.", filter);
            return Enumerable.Empty<AdapterResponseModel<SimplifiedSecretResponse?>>();
        }
        var relevantSecrets = keyVaultSecrets.Where(secret => regex.IsMatch(secret?.Data?.SecretName.ToLower() ?? string.Empty)).ToList();
        var result = relevantSecrets.Select(secret => new AdapterResponseModel<SimplifiedSecretResponse?>
        {
            Status = secret.Status,
            Data = secret.Data
        });
        return result;
    }

    private IEnumerable<Dictionary<string, object>> Filter(
        IEnumerable<Dictionary<string, object>> keyVaultSecrets,
        string filter
        )
    {
        Regex regex;
        try
        {
            regex = new Regex(filter.ToLower(), RegexOptions.None, TimeSpan.FromMilliseconds(5));
        }
        catch (RegexParseException ex)
        {
            logger.LogError(ex, "Couldn't parse and create regex. Value: {value}.", filter);
            return Enumerable.Empty<Dictionary<string, object>>();
        }
        var relevantSecrets = keyVaultSecrets.Where(
            secret => regex.IsMatch(secret?["Name"].ToString()?.ToLower() ?? string.Empty)
            ).ToList();
        return relevantSecrets;
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
        SecretModel secretModel,
        AdapterResponseModel<IEnumerable<AdapterResponseModel<SimplifiedSecretResponse?>>> secretsResultModel,
        CancellationToken cancellationToken
        )
    {
        var secrets = CollectSecrets(secretsResultModel);
        var filteredSecrets = Filter(secrets, secretModel.SecretFilter);
        var deletionCounter1 = 0;
        var deletionCounter2 = 0;

        foreach (var secret in filteredSecrets)
        {
            var secretName = secret?.Data?.SecretName;
            var secretValue = secret?.Data?.SecretValue;
            if (secretName is not null && secretValue is not null)
            {
                deletionCounter1++;

                var request = new SecretRequest<string>()
                {
                    AdditionalData = secretName,
                    ClientId = secretModel.ClientId,
                    ClientSecret = secretModel.ClientSecret,
                    KeyVaultName = secretModel.KeyVaultName,
                    TenantId = secretModel.TenantId
                };
                (var isSuccess, var response) = await adapterCommunicator.CommunicateWithAdapterAsync(
                    request,
                    CommandTypes.DeleteSecretRequest,
                    cancellationToken
                    );

                var deletionStatus = JsonSerializer
                    .Deserialize<BaseResponse<AdapterStatus>>(response)?.Data;

                if (isSuccess && deletionStatus == AdapterStatus.Success)
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
                KeyVaultName = secretModel.KeyVaultName,
                SecretNameRegex = secretModel.SecretFilter,
                User = secretModel.UserName

            };
            await secretChangeColdRepository.AddEntityAsync(entity, cancellationToken);
            return AdapterStatus.Success;
        }

        return AdapterStatus.Unknown;
    }

    private static IEnumerable<AdapterResponseModel<SimplifiedSecretResponse?>> CollectSecrets(
        AdapterResponseModel<IEnumerable<AdapterResponseModel<SimplifiedSecretResponse?>>>? secretsResultModel
        )
    {
        if (secretsResultModel is null)
        {
            return Enumerable.Empty<AdapterResponseModel<SimplifiedSecretResponse?>>();
        }

        var relevantSecrets = secretsResultModel.Data.Where(secret => secret != null);
        var result = relevantSecrets.Select(secret => new AdapterResponseModel<SimplifiedSecretResponse?>
        {
            Status = secret!.Status,
            Data = secret!.Data
        });
        return result;
    }

    private static void CollectSecrets(
        string keyVault,
        List<SecretResult> secretList,
        AdapterResponseModel<SimplifiedSecretResponse?>? filteredSecret
        )
    {
        if (filteredSecret is not null)
        {
            var secretName = filteredSecret.Data?.SecretName ?? string.Empty;
            var secretValue = filteredSecret.Data?.SecretValue ?? string.Empty;

            if (!string.IsNullOrEmpty(secretName) && !string.IsNullOrEmpty(secretValue))
            {
                secretList.Add(new()
                {
                    KeyVault = keyVault,
                    SecretName = secretName,
                    SecretValue = secretValue
                });
            }
        }
    }

    private static AdapterResponseModel<IEnumerable<DeletedSecretResult>> GetResult(
        AdapterStatus status,
        IEnumerable<DeletedSecretResult> secretList
        )
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

    private static BaseSecretRequest GetBaseSecretRequest(SecretModel secretModel)
    {
        return new BaseSecretRequest()
        {
            ClientId = secretModel.ClientId,
            ClientSecret = secretModel.ClientSecret,
            TenantId = secretModel.TenantId,
            KeyVaultName = secretModel.KeyVaultName
        };
    }

    private static BaseSecretRequest GetBaseSecretRequest(SecretBaseModel secretModel)
    {
        return new BaseSecretRequest()
        {
            ClientId = secretModel.ClientId,
            ClientSecret = secretModel.ClientSecret,
            TenantId = secretModel.TenantId,
            KeyVaultName = string.Empty
        };
    }

    private static BaseSecretRequest GetBaseSecretRequest(SecretCopyModel secretModel, bool from)
    {
        return new BaseSecretRequest()
        {
            ClientId = secretModel.ClientId,
            ClientSecret = secretModel.ClientSecret,
            TenantId = secretModel.TenantId,
            KeyVaultName = from ? secretModel.FromKeyVault : secretModel.ToKeyVault
        };
    }
}
