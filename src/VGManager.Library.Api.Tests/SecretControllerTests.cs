using AutoMapper;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Text.Json;
using VGManager.Adapter.Client.Interfaces;
using VGManager.Adapter.Models.Models;
using VGManager.Adapter.Models.Response;
using VGManager.Adapter.Models.StatusEnums;
using VGManager.Library.Api.Endpoints.Secret;
using VGManager.Library.Api.Endpoints.Secret.Request;
using VGManager.Library.Api.Endpoints.Secret.Response;
using VGManager.Library.Api.MapperProfiles;
using VGManager.Library.Api.Tests;
using VGManager.Library.Repositories.DbContexts;
using VGManager.Library.Repositories.SecretRepositories;
using VGManager.Library.Services;

namespace VGManager.Libary.Api.Tests;

[TestFixture]
public class SecretControllerTests
{
    private SecretController _controller;
    private Mock<IVGManagerAdapterClientService> _clientService;

    private OperationsDbContext _operationsDbContext = null!;

    [SetUp]
    public void Setup()
    {
        var mapperConfiguration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(typeof(SecretProfile));
        });

        var mapper = mapperConfiguration.CreateMapper();
        var loggerMock = new Mock<ILogger<KeyVaultService>>();

        _operationsDbContext = DbContextTestBase.CreateDatabaseContext();

        _clientService = new(MockBehavior.Strict);
        var adapterCommunicator = new AdapterCommunicator(_clientService.Object);

        var secretRepository = new SecretChangeColdRepository(_operationsDbContext);
        var keyVaultCopyRepository = new KeyVaultCopyColdRepository(_operationsDbContext);
        var keyVaultService = new KeyVaultService(adapterCommunicator, secretRepository, keyVaultCopyRepository, loggerMock.Object);
        _controller = new(keyVaultService, mapper);
    }

    [Test]
    public async Task GetKeyVaultsAsync_works_well()
    {
        // Arrange
        var request = new SecretBaseRequest
        {
            TenantId = "tenantId1",
            ClientId = "clientId1",
            ClientSecret = "clientSecret",
            UserName = "userName"
        };

        var keyVaultResponse = new BaseResponse<Dictionary<string, object>>()
        {
            Data = new Dictionary<string, object>
            {
                { "Status", new { Name = "1" } },
                { "Data", new Dictionary<string, object>
                {
                    ["subscription"] = "MyOwnSubscription1",
                    ["keyVaults"] = new List<string>
                    {
                        "KeyVaultName1",
                        "KeyVaultName2",
                        "KeyVaultName3"
                    }
                }
                }
            },
        };

        _clientService.Setup(x => x.SendAndReceiveMessageAsync("GetKeyVaultsRequest", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, JsonSerializer.Serialize(keyVaultResponse)));

        // Act
        var result = await _controller.GetKeyVaultsAsync(request, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        _clientService.Verify(x => x.SendAndReceiveMessageAsync("GetKeyVaultsRequest", It.IsAny<string>(), default), Times.Once);
    }

    [Test]
    public async Task GetKeyVaultsAsync_Adapter_returns_success_false()
    {
        // Arrange
        var request = new SecretBaseRequest
        {
            TenantId = "tenantId1",
            ClientId = "clientId1",
            ClientSecret = "clientSecret",
            UserName = "userName"
        };

        _clientService.Setup(x => x.SendAndReceiveMessageAsync("GetKeyVaultsRequest", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, JsonSerializer.Serialize((BaseResponse<Dictionary<string, object>>) null!)));

        // Act
        var result = await _controller.GetKeyVaultsAsync(request, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        _clientService.Verify(x => x.SendAndReceiveMessageAsync("GetKeyVaultsRequest", It.IsAny<string>(), default), Times.Once);
    }

    [Test]
    public async Task GetKeyVaultsAsync_Adapter_returns_null()
    {
        // Arrange
        var request = new SecretBaseRequest
        {
            TenantId = "tenantId1",
            ClientId = "clientId1",
            ClientSecret = "clientSecret",
            UserName = "userName"
        };

        _clientService.Setup(x => x.SendAndReceiveMessageAsync("GetKeyVaultsRequest", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, JsonSerializer.Serialize((BaseResponse<Dictionary<string, object>>)null!)));

        // Act
        var result = await _controller.GetKeyVaultsAsync(request, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        _clientService.Verify(x => x.SendAndReceiveMessageAsync("GetKeyVaultsRequest", It.IsAny<string>(), default), Times.Once);
    }

    [Test]
    public async Task GetAsync_Works_well()
    {
        // Arrange
        var keyVaultName = "KeyVaultName1";
        var tenantId = "tenantId1";
        var clientId = "clientId1";
        var clientSecret = "clientSecret1";
        var secretFilter = "Secret";
        var request = TestSampleData.GetRequest(keyVaultName, secretFilter, tenantId, clientId, clientSecret);

        var secretsGetResponse = TestSampleData.GetSecretsGetResponse();
        var secretResponse = new BaseResponse<AdapterResponseModel<IEnumerable<AdapterResponseModel<SimplifiedSecretResponse?>>>>()
        {
            Data = new AdapterResponseModel<IEnumerable<AdapterResponseModel<SimplifiedSecretResponse?>>>
            {
                Data = new List<AdapterResponseModel<SimplifiedSecretResponse?>>
                {
                    new()
                    {
                        Data = new SimplifiedSecretResponse
                        {
                            SecretName = "SecretFilter123",
                            SecretValue = "3Kpu6gF214vAqHlzaX5G"
                        }
                    },
                    new()
                    {
                        Data = new SimplifiedSecretResponse
                        {
                            SecretName = "SecretFilter456",
                            SecretValue = "KCRQJ08PdFHU9Ly2pUI2"
                        }
                    },
                    new()
                    {
                        Data = new SimplifiedSecretResponse
                        {
                            SecretName = "SecretFilter789",
                            SecretValue = "ggl1oBLSiYNBliNQhsGW"
                        }
                    }
                },
                Status = AdapterStatus.Success
            }
        };

        _clientService.Setup(x => x.SendAndReceiveMessageAsync("GetSecretsRequest", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, JsonSerializer.Serialize(secretResponse)));

        // Act
        var result = await _controller.GetAsync(request, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        ((AdapterResponseModel<IEnumerable<SecretResponse>>)((OkObjectResult)result.Result!).Value!).Should().BeEquivalentTo(secretsGetResponse);

        _clientService.Verify(x => x.SendAndReceiveMessageAsync("GetSecretsRequest", It.IsAny<string>(), default), Times.Once);
    }

    [Test]
    public async Task GetAsync_Adapter_returns_success_false()
    {
        // Arrange
        var keyVaultName = "KeyVaultName1";
        var tenantId = "tenantId1";
        var clientId = "clientId1";
        var clientSecret = "clientSecret1";
        var secretFilter = "Secret";
        var request = TestSampleData.GetRequest(keyVaultName, secretFilter, tenantId, clientId, clientSecret);

        var secretsGetResponse = new AdapterResponseModel<IEnumerable<SecretResponse>>
        {
            Data = Enumerable.Empty<SecretResponse>(),
            Status = AdapterStatus.Unknown
        };

        _clientService.Setup(x => x.SendAndReceiveMessageAsync("GetSecretsRequest", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, JsonSerializer.Serialize(
                (BaseResponse<AdapterResponseModel<IEnumerable<AdapterResponseModel<SimplifiedSecretResponse?>>>>)null!)
            ));

        // Act
        var result = await _controller.GetAsync(request, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        ((AdapterResponseModel<IEnumerable<SecretResponse>>)((OkObjectResult)result.Result!).Value!).Should().BeEquivalentTo(secretsGetResponse);

        _clientService.Verify(x => x.SendAndReceiveMessageAsync("GetSecretsRequest", It.IsAny<string>(), default), Times.Once);
    }

    [Test]
    public async Task GetAsync_Adapter_returns_null()
    {
        // Arrange
        var keyVaultName = "KeyVaultName1";
        var tenantId = "tenantId1";
        var clientId = "clientId1";
        var clientSecret = "clientSecret1";
        var secretFilter = "Secret";
        var request = TestSampleData.GetRequest(keyVaultName, secretFilter, tenantId, clientId, clientSecret);

        var secretsGetResponse = new AdapterResponseModel<IEnumerable<SecretResponse>>
        {
            Data = Enumerable.Empty<SecretResponse>(),
            Status = AdapterStatus.Unknown
        };

        _clientService.Setup(x => x.SendAndReceiveMessageAsync("GetSecretsRequest", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, JsonSerializer.Serialize(
                (BaseResponse<AdapterResponseModel<IEnumerable<AdapterResponseModel<SimplifiedSecretResponse?>>>>)null!)
            ));

        // Act
        var result = await _controller.GetAsync(request, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        ((AdapterResponseModel<IEnumerable<SecretResponse>>)((OkObjectResult)result.Result!).Value!).Should().BeEquivalentTo(secretsGetResponse);

        _clientService.Verify(x => x.SendAndReceiveMessageAsync("GetSecretsRequest", It.IsAny<string>(), default), Times.Once);
    }

    [Test]
    public void GetDeletedAsync_Works_well()
    {
        // Arrange
        var keyVaultName = "KeyVaultName1";
        var tenantId = "tenantId1";
        var clientId = "clientId1";
        var clientSecret = "clientSecret1";
        var secretFilter = "DeletedSecretFilter";
        var request = TestSampleData.GetRequest(keyVaultName, secretFilter, tenantId, clientId, clientSecret);

        var secretResponse = new BaseResponse<AdapterResponseModel<IEnumerable<Dictionary<string, object>>>>()
        {
            Data = new AdapterResponseModel<IEnumerable<Dictionary<string, object>>>
            {
                Data = new List<Dictionary<string, object>>()
                {
                    new()
                    {
                        { "Name", "DeletedSecretFilter123" },
                        { "DeletedOn", "2021-10-01T00:00:00Z" }
                    },
                    new()
                    {
                        { "Name", "DeletedSecretFilter456" },
                        { "DeletedOn", "2022-10-01T00:00:00Z" }
                    }
                },
                Status = AdapterStatus.Success
            }
        };

        _clientService.Setup(x => x.SendAndReceiveMessageAsync("GetDeletedSecretsRequest", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, JsonSerializer.Serialize(secretResponse)));

        // Act
        var result = _controller.GetDeletedAsync(request, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<ActionResult<AdapterResponseModel<IEnumerable<DeletedSecretResponse>>>>();
        _clientService.Verify(x => x.SendAndReceiveMessageAsync("GetDeletedSecretsRequest", It.IsAny<string>(), default), Times.Once);
    }

    [Test]
    public void GetDeletedAsync_Adapter_returns_success_false()
    {
        // Arrange
        var keyVaultName = "KeyVaultName1";
        var tenantId = "tenantId1";
        var clientId = "clientId1";
        var clientSecret = "clientSecret1";
        var secretFilter = "DeletedSecretFilter";
        var request = TestSampleData.GetRequest(keyVaultName, secretFilter, tenantId, clientId, clientSecret);

        _clientService.Setup(x => x.SendAndReceiveMessageAsync("GetDeletedSecretsRequest", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, JsonSerializer.Serialize((BaseResponse<AdapterResponseModel<IEnumerable<Dictionary<string, object>>>>) null!)));

        // Act
        var result = _controller.GetDeletedAsync(request, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<ActionResult<AdapterResponseModel<IEnumerable<DeletedSecretResponse>>>>();
        _clientService.Verify(x => x.SendAndReceiveMessageAsync("GetDeletedSecretsRequest", It.IsAny<string>(), default), Times.Once);
    }

    [Test]
    public void GetDeletedAsync_Adapter_returns_null()
    {
        // Arrange
        var keyVaultName = "KeyVaultName1";
        var tenantId = "tenantId1";
        var clientId = "clientId1";
        var clientSecret = "clientSecret1";
        var secretFilter = "DeletedSecretFilter";
        var request = TestSampleData.GetRequest(keyVaultName, secretFilter, tenantId, clientId, clientSecret);

        _clientService.Setup(x => x.SendAndReceiveMessageAsync("GetDeletedSecretsRequest", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, JsonSerializer.Serialize((BaseResponse<AdapterResponseModel<IEnumerable<Dictionary<string, object>>>>)null!)));

        // Act
        var result = _controller.GetDeletedAsync(request, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<ActionResult<AdapterResponseModel<IEnumerable<DeletedSecretResponse>>>>();
        _clientService.Verify(x => x.SendAndReceiveMessageAsync("GetDeletedSecretsRequest", It.IsAny<string>(), default), Times.Once);
    }

    [Test]
    public async Task DeleteAsync_Works_well()
    {
        // Arrange
        var keyVaultName = "KeyVaultName1";
        var tenantId = "tenantId1";
        var clientId = "clientId1";
        var clientSecret = "clientSecret1";
        var secretFilter = "Secret";
        var request = TestSampleData.GetRequest(keyVaultName, secretFilter, tenantId, clientId, clientSecret);

        var deletionStatus = new BaseResponse<AdapterStatus>()
        {
            Data = AdapterStatus.Success
        };

        var secretsGetResponse = TestSampleData.GetEmptySecretsGetResponse();

        var secretResponse1 = new BaseResponse<AdapterResponseModel<IEnumerable<AdapterResponseModel<SimplifiedSecretResponse?>>>>()
        {
            Data = new AdapterResponseModel<IEnumerable<AdapterResponseModel<SimplifiedSecretResponse?>>>
            {
                Data = new List<AdapterResponseModel<SimplifiedSecretResponse?>>
                {
                    new()
                    {
                        Data = new SimplifiedSecretResponse
                        {
                            SecretName = "SecretFilter123",
                            SecretValue = "3Kpu6gF214vAqHlzaX5G"
                        }
                    },
                    new()
                    {
                        Data = new SimplifiedSecretResponse
                        {
                            SecretName = "SecretFilter456",
                            SecretValue = "KCRQJ08PdFHU9Ly2pUI2"
                        }
                    },
                    new()
                    {
                        Data = new SimplifiedSecretResponse
                        {
                            SecretName = "SecretFilter789",
                            SecretValue = "ggl1oBLSiYNBliNQhsGW"
                        }
                    }
                },
                Status = AdapterStatus.Success
            }
        };

        var secretResponse2 = new BaseResponse<AdapterResponseModel<IEnumerable<AdapterResponseModel<SimplifiedSecretResponse?>>>>()
        {
            Data = new AdapterResponseModel<IEnumerable<AdapterResponseModel<SimplifiedSecretResponse?>>>
            {
                Data = Enumerable.Empty<AdapterResponseModel<SimplifiedSecretResponse?>>(),
                Status = AdapterStatus.Success
            }
        };

        _clientService.SetupSequence(x => x.SendAndReceiveMessageAsync("GetSecretsRequest", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, JsonSerializer.Serialize(secretResponse1)))
            .ReturnsAsync((true, JsonSerializer.Serialize(secretResponse2)));

        _clientService.Setup(x => x.SendAndReceiveMessageAsync("DeleteSecretRequest", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, JsonSerializer.Serialize(deletionStatus)));

        // Act
        var result = await _controller.DeleteAsync(request, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        ((AdapterResponseModel<IEnumerable<SecretResponse>>)((OkObjectResult)result.Result!).Value!).Should().BeEquivalentTo(secretsGetResponse);

        _clientService.Verify(x => x.SendAndReceiveMessageAsync("GetSecretsRequest", It.IsAny<string>(), default), Times.Exactly(2));
        _clientService.Verify(x => x.SendAndReceiveMessageAsync("DeleteSecretRequest", It.IsAny<string>(), default), Times.Exactly(3));
    }

    [Test]
    public async Task DeleteAsync_Adapter_returns_success_false()
    {
        // Arrange
        var keyVaultName = "KeyVaultName1";
        var tenantId = "tenantId1";
        var clientId = "clientId1";
        var clientSecret = "clientSecret1";
        var secretFilter = "Secret";
        var request = TestSampleData.GetRequest(keyVaultName, secretFilter, tenantId, clientId, clientSecret);

        var secretsGetResponse = TestSampleData.GetEmptySecretsGetResponse();
        secretsGetResponse.Status = AdapterStatus.Unknown;

        _clientService
            .Setup(x => x.SendAndReceiveMessageAsync("GetSecretsRequest", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((
                false, 
                JsonSerializer.Serialize((BaseResponse<AdapterResponseModel<IEnumerable<AdapterResponseModel<SimplifiedSecretResponse?>>>>)null!
                )));

        // Act
        var result = await _controller.DeleteAsync(request, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        ((AdapterResponseModel<IEnumerable<SecretResponse>>)((OkObjectResult)result.Result!).Value!).Should().BeEquivalentTo(secretsGetResponse);

        _clientService.Verify(x => x.SendAndReceiveMessageAsync("GetSecretsRequest", It.IsAny<string>(), default), Times.Exactly(2));
        _clientService.Verify(x => x.SendAndReceiveMessageAsync("DeleteSecretRequest", It.IsAny<string>(), default), Times.Never);
    }

    [Test]
    public async Task DeleteAsync_Adapter_returns_null()
    {
        // Arrange
        var keyVaultName = "KeyVaultName1";
        var tenantId = "tenantId1";
        var clientId = "clientId1";
        var clientSecret = "clientSecret1";
        var secretFilter = "Secret";
        var request = TestSampleData.GetRequest(keyVaultName, secretFilter, tenantId, clientId, clientSecret);

        var secretsGetResponse = TestSampleData.GetEmptySecretsGetResponse();
        secretsGetResponse.Status = AdapterStatus.Unknown;

        _clientService
            .Setup(x => x.SendAndReceiveMessageAsync("GetSecretsRequest", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((
                true,
                JsonSerializer.Serialize((BaseResponse<AdapterResponseModel<IEnumerable<AdapterResponseModel<SimplifiedSecretResponse?>>>>)null!
                )));

        // Act
        var result = await _controller.DeleteAsync(request, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        ((AdapterResponseModel<IEnumerable<SecretResponse>>)((OkObjectResult)result.Result!).Value!).Should().BeEquivalentTo(secretsGetResponse);

        _clientService.Verify(x => x.SendAndReceiveMessageAsync("GetSecretsRequest", It.IsAny<string>(), default), Times.Exactly(2));
        _clientService.Verify(x => x.SendAndReceiveMessageAsync("DeleteSecretRequest", It.IsAny<string>(), default), Times.Never);
    }

    [Test]
    public async Task DeleteInlineAsync_Works_well()
    {
        // Arrange
        var keyVaultName = "KeyVaultName1";
        var tenantId = "tenantId1";
        var clientId = "clientId1";
        var clientSecret = "clientSecret1";
        var secretFilter = "Secret";
        var request = TestSampleData.GetRequest(keyVaultName, secretFilter, tenantId, clientId, clientSecret);

        var deletionStatus = new BaseResponse<AdapterStatus>()
        {
            Data = AdapterStatus.Success
        };

        var secretResponse = new BaseResponse<AdapterResponseModel<IEnumerable<AdapterResponseModel<SimplifiedSecretResponse?>>>>()
        {
            Data = new AdapterResponseModel<IEnumerable<AdapterResponseModel<SimplifiedSecretResponse?>>>
            {
                Data = new List<AdapterResponseModel<SimplifiedSecretResponse?>>
                {
                    new()
                    {
                        Data = new SimplifiedSecretResponse
                        {
                            SecretName = "SecretFilter123",
                            SecretValue = "3Kpu6gF214vAqHlzaX5G"
                        }
                    },
                    new()
                    {
                        Data = new SimplifiedSecretResponse
                        {
                            SecretName = "SecretFilter456",
                            SecretValue = "KCRQJ08PdFHU9Ly2pUI2"
                        }
                    },
                    new()
                    {
                        Data = new SimplifiedSecretResponse
                        {
                            SecretName = "SecretFilter789",
                            SecretValue = "ggl1oBLSiYNBliNQhsGW"
                        }
                    }
                },
                Status = AdapterStatus.Success
            }
        };

        _clientService.Setup(x => x.SendAndReceiveMessageAsync("GetSecretsRequest", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, JsonSerializer.Serialize(secretResponse)));

        _clientService.Setup(x => x.SendAndReceiveMessageAsync("DeleteSecretRequest", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, JsonSerializer.Serialize(deletionStatus)));

        // Act
        var result = await _controller.DeleteInlineAsync(request, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();

        _clientService.Verify(x => x.SendAndReceiveMessageAsync("GetSecretsRequest", It.IsAny<string>(), default), Times.Once);
        _clientService.Verify(x => x.SendAndReceiveMessageAsync("DeleteSecretRequest", It.IsAny<string>(), default), Times.Exactly(3));
    }

    [Test]
    public void RecoverAsync_Works_well()
    {
        // Arrange
        var keyVaultName = "KeyVaultName1";
        var tenantId = "tenantId1";
        var clientId = "clientId1";
        var clientSecret = "clientSecret1";
        var secretFilter = "DeletedSecretFilter";
        var request = TestSampleData.GetRequest(keyVaultName, secretFilter, tenantId, clientId, clientSecret);

        var secretsGetResponse = TestSampleData.GetEmptySecretsGetResponse1();

        var recoverStatus = new BaseResponse<AdapterStatus>()
        {
            Data = AdapterStatus.Success
        };

        var deletedSecretResponse1 = new BaseResponse<AdapterResponseModel<IEnumerable<Dictionary<string, object>>>>()
        {
            Data = new AdapterResponseModel<IEnumerable<Dictionary<string, object>>>
            {
                Data = new List<Dictionary<string, object>>()
                {
                    new()
                    {
                        { "Name", "DeletedSecretFilter123" },
                        { "DeletedOn", "2021-10-01T00:00:00Z" }
                    },
                    new()
                    {
                        { "Name", "DeletedSecretFilter456" },
                        { "DeletedOn", "2022-10-01T00:00:00Z" }
                    }
                },
                Status = AdapterStatus.Success
            }
        };

        var deletedSecretResponse2 = new BaseResponse<AdapterResponseModel<IEnumerable<Dictionary<string, object>>>>()
        {
            Data = new AdapterResponseModel<IEnumerable<Dictionary<string, object>>>
            {
                Data = Enumerable.Empty<Dictionary<string, object>>(),
                Status = AdapterStatus.Success
            }
        };

        _clientService.SetupSequence(
                x => x.SendAndReceiveMessageAsync("GetDeletedSecretsRequest", It.IsAny<string>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((true, JsonSerializer.Serialize(deletedSecretResponse1)))
            .ReturnsAsync((true, JsonSerializer.Serialize(deletedSecretResponse2)));

        _clientService.Setup(x => x.SendAndReceiveMessageAsync("RecoverSecretRequest", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, JsonSerializer.Serialize(recoverStatus)));

        // Act
        var result = _controller.RecoverAsync(request, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Result.Should().BeOfType<OkObjectResult>();
        ((AdapterResponseModel<IEnumerable<DeletedSecretResponse>>)((OkObjectResult)result.Result!.Result!).Value!)
            .Should()
            .BeEquivalentTo(secretsGetResponse);

        _clientService.Verify(x => x.SendAndReceiveMessageAsync("GetDeletedSecretsRequest", It.IsAny<string>(), default), Times.Exactly(2));
        _clientService.Verify(x => x.SendAndReceiveMessageAsync("RecoverSecretRequest", It.IsAny<string>(), default), Times.Exactly(2));
    }

    [Test]
    public void RecoverInlineAsync_Works_well()
    {
        // Arrange
        var keyVaultName = "KeyVaultName1";
        var tenantId = "tenantId1";
        var clientId = "clientId1";
        var clientSecret = "clientSecret1";
        var secretFilter = "DeletedSecretFilter";
        var request = TestSampleData.GetRequest(keyVaultName, secretFilter, tenantId, clientId, clientSecret);

        var recoverStatus = new BaseResponse<AdapterStatus>()
        {
            Data = AdapterStatus.Success
        };

        var deletedSecretResponse = new BaseResponse<AdapterResponseModel<IEnumerable<Dictionary<string, object>>>>()
        {
            Data = new AdapterResponseModel<IEnumerable<Dictionary<string, object>>>
            {
                Data = new List<Dictionary<string, object>>()
                {
                    new()
                    {
                        { "Name", "DeletedSecretFilter123" },
                        { "DeletedOn", "2021-10-01T00:00:00Z" }
                    },
                    new()
                    {
                        { "Name", "DeletedSecretFilter456" },
                        { "DeletedOn", "2022-10-01T00:00:00Z" }
                    }
                },
                Status = AdapterStatus.Success
            }
        };

        _clientService.SetupSequence(
                x => x.SendAndReceiveMessageAsync("GetDeletedSecretsRequest", It.IsAny<string>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((true, JsonSerializer.Serialize(deletedSecretResponse)));

        _clientService.Setup(x => x.SendAndReceiveMessageAsync("RecoverSecretRequest", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, JsonSerializer.Serialize(recoverStatus)));

        // Act
        var result = _controller.RecoverInlineAsync(request, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Result.Should().BeOfType<OkObjectResult>();
        ((AdapterStatus)((OkObjectResult)result.Result!.Result!).Value!).Should().Be(AdapterStatus.Success);

        _clientService.Verify(x => x.SendAndReceiveMessageAsync("GetDeletedSecretsRequest", It.IsAny<string>(), default), Times.Once);
        _clientService.Verify(x => x.SendAndReceiveMessageAsync("RecoverSecretRequest", It.IsAny<string>(), default), Times.Exactly(2));
    }

    [Test]
    public async Task CopyAsync_Works_well()
    {
        // Arrange
        var fromKeyVault = "KeyVaultName1";
        var tenantId = "tenantId1";
        var clientId = "clientId1";
        var clientSecret = "clientSecret1";
        var ToKeyVault = "ToKeyVault";
        var request = TestSampleData.GetRequest(fromKeyVault, ToKeyVault, tenantId, clientId, clientSecret, true);

        var secretResponse1 = new BaseResponse<AdapterResponseModel<IEnumerable<KeyVaultSecret>>>()
        {
            Data = new AdapterResponseModel<IEnumerable<KeyVaultSecret>>
            {
                Data = new List<KeyVaultSecret>
                {
                    new("SecretFilter123", "3Kpu6gF214vAqHlzaX5G"),
                    new("SecretFilter456", "KCRQJ08PdFHU9Ly2pUI2"),
                    new("SecretFilter789", "ggl1oBLSiYNBliNQhsGW")
                },
                Status = AdapterStatus.Success
            }
        };

        var secretResponse2 = new BaseResponse<AdapterResponseModel<IEnumerable<KeyVaultSecret>>>()
        {
            Data = new AdapterResponseModel<IEnumerable<KeyVaultSecret>>
            {
                Data = Enumerable.Empty<KeyVaultSecret>(),
                Status = AdapterStatus.Success
            }
        };

        var addStatus = new BaseResponse<AdapterStatus>()
        {
            Data = AdapterStatus.Success
        };

        _clientService.SetupSequence(
                x => x.SendAndReceiveMessageAsync("GetAllSecretsRequest", It.IsAny<string>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((true, JsonSerializer.Serialize(secretResponse1)))
            .ReturnsAsync((true, JsonSerializer.Serialize(secretResponse2)));

        _clientService.Setup(x => x.SendAndReceiveMessageAsync("AddKeyVaultSecretRequest", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, JsonSerializer.Serialize(addStatus)));

        // Act
        var result = await _controller.CopyAsync(request, default);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        ((AdapterStatus)((OkObjectResult)result.Result!).Value!).Should().Be(AdapterStatus.Success);

        _clientService.Verify(x => x.SendAndReceiveMessageAsync("GetAllSecretsRequest", It.IsAny<string>(), default), Times.Exactly(2));
        _clientService.Verify(x => x.SendAndReceiveMessageAsync("AddKeyVaultSecretRequest", It.IsAny<string>(), default), Times.Exactly(3));
    }

    [TearDown]
    public async Task TearDown()
    {
        DbContextTestBase.TearDownDatabaseContext(_operationsDbContext);
        await _operationsDbContext.DisposeAsync();
    }
}
